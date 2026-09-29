# RegressionLibrary

## What this is

**AlphaPascal.Math.Regression** (NuGet id — see `ClassLibraryMath\Regression.csproj`): finds
polynomial regression/approximation coefficients and interpolation, by reducing the fit to a
linear system and solving it.

- Single-variable power polynomials: `Approximators\Approximator.cs`
- Two-factor polynomial regression: `Two-factor regression\*`
- Interpolation: `Interpolators\*`

## Two solvers being compared

- `Regression.MathService.Gaus` — hand-rolled Gaussian elimination, used internally by
  `Two-factor regression\Implements\Solver.cs` and `Approximators\Approximator.cs`.
- `Regression.Two_factor_regression.Implements.SolverMathNet` — wraps
  `MathNet.Numerics.LinearAlgebra`'s `Matrix.Solve()`.

Both implement `ISolverSystem`.

## Two-factor regression pipeline

`DataTwoFact` (X1, X2, Y)
→ `ExpressionCreator.CreateTwoOrder/ThirdOrder/FourthOrderPolynomialExpression` builds a
  symbolic sum-of-squared-residuals expression via MathNet.Symbolics (9/16/25 coefficients). Term
  order matches `Second/Third/FourthOrderBasisExponents`, and each lower order is a positional
  prefix of the next (issue #15, locked by `BasisExponentsOrderTests`)
→ `DerivativeCalculator` differentiates it per coefficient
→ `ApproximationService.BuildMatrix` assembles the normal-equations matrices
→ `ISolverSystem.GetRoots` solves for the coefficients.

That pipeline (`IRegressionAnalysisService` / `ApproximationService`) is the legacy path, kept
untouched for backward compatibility.

## Polynomial fit solvers (recommended path)

`IPolynomialFitService.GetValues(IEnumerable<DataTwoFact>)` is the common contract for fitting a
two-factor polynomial over an `IBasisExponents` (2nd/3rd/4th order, 9/16/25 coefficients),
returning original-monomial coefficients in the basis's fixed order (which goes to sensor
firmware as is). Downstream consumers build a list of
`Func<List<DataTwoFact>, IEnumerable<double>>` variants from these solvers.

- `PolynomialLeastSquaresSolver : ILeastSquaresRegressionService` (which inherits
  `IPolynomialFitService` with no members of its own, so consumers of the published package keep
  compiling) — minimizes the sum of squares. **Recommended default.** It generalizes better to
  temperatures between calibration series.
- `MinimaxPolynomialSolver : IPolynomialFitService` (deliberately not the least-squares
  interface) — minimizes the max absolute error over the fitted points via Lawson IRLS
  (`maxIterations` default 1000, `relativeTolerance` default 1e-6 over a 200-iteration window).
  Lower in-sample `GetMax()`, but worse leave-one-series-out generalization, and it follows
  noisy points. Use only when acceptance is checked strictly at the calibration points.
  Numbers: `SolversTests\ApproximationServiceTests\ThirdOrder\MinimaxPolynomialSolverTests.cs`.
- Both share the internal `CenteredPolynomialBasis` (mean/scale centering, design matrix,
  symbolic conversion back to the original basis). The symbolic conversion is slow, so it runs
  exactly once per fit. Never call it inside the minimax iteration loop.

## IsothermalSeriesOutlierDetector (issue #21)

`Regression.OutlierDetection`, `ClassLibraryMath\OutlierDetection\` — finds mis-loaded points
(wrong/forgotten dead weight) in **one isothermal series** of `DataTwoFact` (X1 = pressure code,
Y = target pressure the loads are meant to reproduce, all Y on one scale; X2 ignored).

Error model (clarified by the repo owner after the #21 spec, which had it the other way round):
Y is taken as exact and goes into the model as recorded; a loading mistake reproduces a
different pressure, so the error shows up in **X1** at an unchanged Y. Hence the detector fits
the characteristic X1 = g(Y), 2nd degree, by QR on centered Y (no normal equations, no
conversion back to the original basis, so not `PolynomialLeastSquaresSolver`), and searches
every subset of up to min(4, n − 5) points for the smallest one whose removal leaves all code
residuals ≤ τ while every removed point misses by > τ. Tests inject errors into X1.

τ = relativeTolerance (default 5e-5) × (Ymax − Ymin) × |Theil–Sen slope of X1 over Y|, in
codes. The scale comes from Y because Y is exact and X1 contains the outliers: a gross end error
changes the code range (X1[10] += 30 on 235 / 15 °C: −10%) but not τ. On clean data it equals the
code range within 0.02%. τ is sensor noise, not half a weight. Measured clean margin: worst
residual / scale is 0.23–1.29·10⁻⁵ (235), 1.85–2.22·10⁻⁵ (223), 1.73–2.59·10⁻⁵ (224), so the
default is 1.9× the worst series.

- Empty list = clean; all `Outlier` = unique minimal set (`CodeError` = X1 − g(Y), in codes);
  all `AmbiguousPoint` = union of several equally good sets; `SeriesNotResolvableException`
  (`MaxOutliers`, `Tolerance`) = too many errors, re-measure.
- Don't replace the exhaustive search with "drop the largest residual and refit": least squares
  smears an end-point error or a shifted tail over the series.
- Known limit (separate follow-up, not solved): on unevenly spaced series an isolated end point
  has leverage ≈ 1 and can absorb its own error, giving a confident but wrong `Outlier` set.
- Tests in `SolversTests\OutlierDetection\`: class = number of errors (`CleanSampleTests`,
  `SinglePointSearchTests`, `TwoPointSearchTests`, plus `ExceptionTests`), method = sample
  (`Sensor235At15C`, `Sensor223Series1`, …), `InlineData` = where and how big the error is, in
  codes. Samples are visible in `Sensor235Samples` / `Sensor223Samples` / `Sensor224Samples`
  (`Sensor223And224Data` is built from them). Scope is deliberately 0 / 1 / 2 errors + the
  exception: more than 2 loading mistakes per series is rare. The owner wants tests this simple
  and the new code without comments; add scenarios only when asked.

## ApproximationCalculationError

`Regression.ErrorAnalysis` namespace, `ClassLibraryMath\ErrorAnalysis\ApproximationCalculationError.cs`
— shared error-analysis class with two constructor-selected modes:

- two-factor: `IEnumerable<double> coefficients` + `IEnumerable<DataTwoFact> data`
- single-variable approximation: `coefficients` + `double[] x, y`

`GetMax()` / `GetCurrent(...)` compute residual error against stored data; calling the
wrong-mode method throws `InvalidOperationException`.

## Test layout

`SolversTests` (xunit):

- `GausTests.cs` / `QRfactorized.cs` — test the raw solvers directly.
- `SolversTests\ApproximationServiceTests\` — `ApproximationServiceTests.cs` (original dataset),
  plus `ApproximationServiceTests223.cs` and `ApproximationServiceTests224.cs`, whose data lives in
  `Sensor223And224Data`, concatenated from `Sensor223Samples` / `Sensor224Samples` (from Excel
  exports `223.xlsx`/`224.xlsx`, Лист1, columns A/B/C = X1/X2/Y, rows 2-56; 5 series of 11
  points each), running the same
  `MaxErrorIsBelowTolerance()`-style test. `PolynomialLeastSquaresSolver`'s `GetMax()` must be
  under 0.011 on both. The legacy `ApproximationService` path must also be under 0.011 on 224,
  but on 223 it is asserted against its observed error (see below).

## Resolved: dataset 223 (issue #5, closed)

The legacy normal-equations path (`ApproximationService` + `SolverMathNet`/`Solver`) gives
wrong coefficients on 223 (`GetMax()` MathNet ~0.043, Gaus ~0.224). Its normal-equations
matrix is ill-conditioned (~1E+32, on 224 too; `ConditionNumberHypothesisTests`). The fix
is `PolynomialLeastSquaresSolver`: QR on a centered/scaled design matrix, ~0.0015 on 223,
matching the MathCad reference to ~6 significant digits. The legacy path is left unchanged for
backward compatibility, so `ApproximationServiceTests223` asserts it against its observed error
(< 0.05 / < 0.25) and the 0.011 criterion against the new solver. Don't "fix" those bounds
either way.

## Current open issues

- [#10](https://github.com/Georgiy-smr/RegressionLibrary/issues/10): sensor 235 misses the
  0.003 tolerance at temperatures between calibration series. This is a model/data problem, not a
  solver bug. The `SeriesCountAnalysis` tests document it, and the fourth-order
  leave-one-series-out test is intentionally red.
- [#21](https://github.com/Georgiy-smr/RegressionLibrary/issues/21): `IsothermalSeriesOutlierDetector`
  is implemented (see above). Still open: whether to change the algorithm or the spec for the
  ~10τ limit (wrong `Outlier` sets on unevenly spaced series).

## Workflow conventions observed in this repo's history

- Work happens on feature branches off `main`, one PR per change via `gh pr create`.
- Commits happen even when a new test intentionally fails — documenting a bug is a valid commit,
  don't hold commits hostage to green tests.
- Don't loosen test tolerances just to make a red test pass.

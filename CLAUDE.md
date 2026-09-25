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
  plus `ApproximationServiceTests223.cs` and `ApproximationServiceTests224.cs`, each a
  self-contained class with its own `_data` field sourced from an Excel export
  (`223.xlsx`/`224.xlsx`, Лист1, columns A/B/C = X1/X2/Y, rows 2-56), running the same
  `MaxErrorIsBelowTolerance()`-style test (both solvers' `GetMax()` error must be under 0.011).

## Known open bug

Dataset 223 fails `MaxErrorIsBelowTolerance()` for both solvers (MathNet ~0.043, Gaus ~0.224
vs a 0.011 tolerance) while 224 passes, and an independently-computed MathCad reference for 223
comes in at ~0.0015 — well within tolerance. Tracked in
[issue #5](https://github.com/Georgiy-smr/RegressionLibrary/issues/5). Current hypothesis:
normal-equations ill-conditioning from uncentered X1/X2 values raised to high powers. **Not yet
fixed** — read the issue before attempting a fix, don't just loosen tolerances.

## Workflow conventions observed in this repo's history

- Work happens on feature branches off `main`, one PR per change via `gh pr create`.
- Commits happen even when a new test intentionally fails — documenting a bug is a valid commit,
  don't hold commits hostage to green tests.
- Don't loosen test tolerances just to make a red test pass.

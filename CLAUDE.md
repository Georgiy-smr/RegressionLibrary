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

The constructor parameter is `accuracyClassPercent`, the sensor's accuracy class in % of range
(default 0.01). τ is a metrological requirement set by the owner, not a number tuned on data:
half the permissible error, reduced to the range, in codes:
τ = (accuracyClassPercent / 100 / 2) × (Ymax − Ymin) × |Theil–Sen slope of X1 over Y|.
The scale comes from Y because Y is exact and X1 contains the outliers: a gross end error changes
the code range (X1[10] += 30 on 235 / 15 °C: −10%) but not τ. On clean data (Ymax − Ymin)·|s|
equals the code range within 0.02%. Consequence, intended: a point off by more than half the
permissible error is suspicious even if the cause is sensor noise, so a sensor whose noise is
close to its requirement (e.g. colleague sample 4) gets flagged points. Fact: with class 0.01%
all 15 real series are clean; the worst uses ~52% of τ (224 series 5).

- Empty list = clean; all `Outlier` = unique minimal set (`CodeError` = X1 − g(Y), in codes);
  all `AmbiguousPoint` = union of several equally good sets; `SeriesNotResolvableException`
  (`MaxOutliers`, `Tolerance`) = too many errors, re-measure.
- Don't replace the exhaustive search with "drop the largest residual and refit": least squares
  smears an end-point error or a shifted tail over the series.
- Unverifiable points: a kept point with leverage > `MaxLeverage` (0.9; hat-matrix diagonal of
  the kept rows, from a thin QR) can't be checked by the rest of the series, because the curve
  passes through it. The whole result then becomes `AmbiguousPoint` (union of the accepted sets'
  removed points plus the unverifiable points), even at k = 0. An `Outlier` result only comes
  from a unique accepted set with no unverifiable kept point. Max kept leverage in the real tests:
  0.78 (235), 0.79 (223, 224), reached when a single error at point 9 is removed and point 10 is
  left alone at the end.
- Tests in `SolversTests\OutlierDetection\`: class = number of errors (`CleanSampleTests`,
  `SinglePointSearchTests`, `TwoPointSearchTests`, plus `ExceptionTests`), method = sample
  (`Sensor235At15C`, `Sensor223Series1`, …), `InlineData` = where and how big the error is, in
  codes. Samples are visible in `Sensor235Samples` / `Sensor223Samples` / `Sensor224Samples`
  (`Sensor223And224Data` is built from them). 0, 1 and 2 errors are tested on all 15 real
  series (1 error: every position 0–10; 2 errors: 6 position pairs), plus the exception. Scope is
  deliberately limited to that: more than 2 loading mistakes per series is rare.
  `Synthetic\` (namespace `SolversTests.Synthetic`) holds hand-built unevenly spaced samples
  (`SyntheticSamples`: MildlyUneven, VacuumWithGap, IsolatedEnd, IsolatedStart) with the same
  class/method/row layout. They're kept apart from real data on purpose. On the isolated samples
  the tests assert a safe result (`SafeResult.AssertSafe`): exact `Outlier`s or `AmbiguousPoint`s
  covering every injected point. The owner wants tests this simple
  and the new code without comments; add scenarios only when asked.

## CalibrationDatasetChecker (issue #24)

`Regression.OutlierDetection`, `ClassLibraryMath\OutlierDetection\CalibrationDatasetChecker.cs`
and `DatasetCheckResult.cs`. It checks a whole sensor dataset (all temperatures) as a layer on
top of the unchanged `IsothermalSeriesOutlierDetector`. The consumer is TwoFactRegressCalc
(Excel columns A = pressure code, B = temperature code, C = pressure, D = temperature). The logic
lives here; the app only calls it and shows the result.

- Input: `CalibrationPoint(PressureCode, TemperatureCode, Pressure, Temperature)`, mapped to the
  detector as X1 / X2 / Y.
- Splitting (owner's decision): by the **measured temperature**, not the temperature code, which
  drifts with pressure. Points are sorted by temperature, and a new series starts where
  neighbours differ by more than `seriesTemperatureGap` (default 2 °C). Input order doesn't
  matter; inside a series the points keep input order.
- Nominal temperature = median of the series' temperatures, `Math.Round(…, AwayFromZero)` to 1 °C.
  Series are ordered by it.
- Status per series: `CheckedSeries` (detector ran; empty = clean), `UnresolvableSeries`
  (`SeriesNotResolvableException`), `SkippedSeries` (detector threw `ArgumentException`, e.g. < 5
  points; `Reason` = its message). One bad series never stops the others.
- `Rows` / `DatasetSuspiciousPoint.Row` are 0-based positions in the input; `Detail.Index` stays
  series-local. `SuspiciousPoints` is flattened and ordered by `Row`. `IsClean` = every series is
  a `CheckedSeries` with no suspicious points (an empty dataset is clean).
- Tests: `SolversTests\OutlierDetection\Dataset\` (namespace `SolversTests.Dataset`), one class
  per scenario (`SplittingTests`, `OneErrorTests`, `TwoSeriesErrorTests`,
  `UnresolvableSeriesTests`, `ShortSeriesTests`, `DatasetValidationTests`). The dataset is
  `Sensor235Dataset`: the 5 sensor-235 samples at 15/18/21/24.5/28 °C, rows 0–54 in that order,
  with a fixed per-point temperature jitter whose median is 0, so the nominals are 15, 18, 21,
  25, 28.
- The splitting and the nominal temperature live in the internal `TemperatureSeriesSplitter`,
  shared with `SectorSensorCharacterizer`.

## SectorCompensation (issue #26, experimental)

`Regression.SectorCompensation`, `ClassLibraryMath\SectorCompensation\`. A sector-based pressure
compensation method, compared with our method on sensor No. 00249979. The algorithm is described
step by step in [`docs\SectorSensorAlgorithm.md`](docs/SectorSensorAlgorithm.md) (in Russian); a
colleague reproduces it in Excel from that document, so `SectorSensor.GetPressure` must keep
matching it. Units don't matter to the method; don't mention them in code or tests.

- `ISensorModel.GetPressure(pressureCode, temperatureCode)` is the common contract. A model only
  stores coefficients; fitting is done by an `ISensorCharacterizer`
  (`Characterize(IEnumerable<CalibrationPoint>)`). No static factory methods.
- `PolynomialSensor` / `PolynomialSensorCharacterizer` — our method: 16 pressure + 9 temperature
  coefficients, both fitted by `PolynomialLeastSquaresSolver` on the same points (Y = pressure /
  temperature) and evaluated through `TwoFactorPolynomialValue`.
- `SectorSensor` / `SectorSensorCharacterizer`:
  1. Rough stage: the embedded `PolynomialSensor` gives rough P and T from both codes.
  2. Triangle selection in (P, T): first triangle in storage order that contains the point (edges
     count as inside); outside the grid, the triangle with the nearest centre, in coordinates
     normalized by the grid range.
  3. Fine stage: sector polynomial `code = c0 + c1·dP + c2·dT + c3·dP² + c4·dT² + c5·dP·dT` from
     the sector's anchor node, solved for dP in closed form (no iterations), root closest to the
     rough dP.
  4. One refinement: if the resulting (P, T) is inside a different triangle, step 3 is repeated
     there.
- Grid: series are split like in `CalibrationDatasetChecker`; nodes per axis must be odd and ≥ 3,
  and all series must have the same pressures (`ArgumentException`). Node coordinates are the
  reference pressure and the temperature **computed by the rough-stage temperature polynomial**
  from the node's codes (not the nominal), so characterization and operation use the same
  temperature. Quadrants of 2×2 cells are cut by the diagonal into a lower and an upper triangle
  with 6 nodes each; the coefficients are the exact 6×6 solution. Sectors are numbered from 1,
  along pressure first, then temperature, lower (odd) before upper (even), as in the document.
- `SectorSensor.Trace` returns the rough P and T, the number of the sector that produced the
  result and the final pressure.
- Stored coefficients (issue #28): the result of a characterization is what `SectorSensor` holds,
  and the consumer (TwoFactRegressCalc) takes the coefficients from it and saves them anywhere
  (Excel, file, sensor). `GetPressure` is mainly for checking our methods inside the library.
  This is independent of the legacy path: no `DataTwoFact`, `IPolynomialFitService` or
  `GetValues`. Two tables:

  | Table | Source | Content |
  |---|---|---|
  | Rough stage | `SectorSensor.RoughSensor` (`PolynomialSensor`) | `PressureCoefficients` a0…a15, `TemperatureCoefficients` b0…b8 (`TwoFactorPolynomialValue` order) |
  | Sector table | `SectorSensor.Sectors`, one `Sector` per row, in the order of their numbers | N, P1, T1, P2, T2, P3, T3, c0…c5 |

  - `Sector` (immutable class): `Number` (1-based = position in `Sectors` + 1), `Vertex1`,
    `Vertex2`, `Vertex3` (`SectorNode(Pressure, Temperature)`), `Coefficients` (exactly 6,
    otherwise `ArgumentException`). **`Vertex1` is the anchor node**: dP and dT are measured from
    it, there is no separate P0, T0.
  - `Sector.Values` = the 12 numbers of the row after N: P1, T1, P2, T2, P3, T3, c0…c5.
  - `SectorSensor.StoredValueCount` = 25 + 12 × number of sectors (265 for 5×11, 121 for 5×5):
    how many numbers have to be stored for the sensor to work. `CoefficientCount` keeps its
    meaning (25 + 6 × sectors).
  - Rebuilding from saved values: `new SectorSensor(new PolynomialSensor(pressure, temperature),
    sectors)`, each sector as `new Sector(number, vertex1, vertex2, vertex3, coefficients)` from
    its saved row. The constructor checks that the sector numbers are 1, 2, 3, … in order
    (`ArgumentException`). `SectorValuesTests` checks that the rebuilt sensor gives bitwise the
    same `GetPressure` on all 279 points of the validation grid.
  - The `SectorSensorCoefficientsReport` test prints both tables tab-separated for Excel.
- Tests: `SolversTests\SensorComparison\` (`NodeTests`, `ContinuityTests`,
  `SyntheticQuadraticTests`, `UniformGridFormulaTests`, `GridValidationTests`,
  `WorkedExampleTests` = points А, Б, В of the document, `SensorComparisonTests`,
  `SectorValuesTests`). Data:
  `Sensor00249979Dataset` (55 calibration points) and `Sensor00249979ValidationGrid` (9 × 31).
- Comparison on the 224 validation points outside the calibration sample (error in units of the
  reference pressure; asserted against the Python prototype within 0.0002):

  | Variant | Coefficients | Max error | Mean error |
  |---|---|---|---|
  | PolynomialSensor | 25 | 0.0037 | 0.0010 |
  | SectorSensor 5×11 | 145 | 0.0046 | 0.0011 |
  | SectorSensor 5×5 (pressure nodes 0, 2, 4, 7, 10) | 73 | 0.0126 | 0.0040 |

- `ClassLibraryMath\Packaging\Descriptions.md` is not updated while the method is experimental.

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
- [#24](https://github.com/Georgiy-smr/RegressionLibrary/issues/24): `CalibrationDatasetChecker`
  for a whole dataset (see above). #21 (the single-series detector) is closed.
- [#28](https://github.com/Georgiy-smr/RegressionLibrary/issues/28): convenient access to the
  sector method's coefficients (see above). #26 (the sector method itself) is closed.

## Workflow conventions observed in this repo's history

- Work happens on feature branches off `main`, one PR per change via `gh pr create`.
- Commits happen even when a new test intentionally fails — documenting a bug is a valid commit,
  don't hold commits hostage to green tests.
- Don't loosen test tolerances just to make a red test pass.

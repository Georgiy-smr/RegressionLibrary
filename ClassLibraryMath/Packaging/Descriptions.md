Библиотека работы с статистикой

# AlphaPascal.Math.Regression

Polynomial regression/approximation and interpolation.

## Two-factor polynomial fit (X1, X2 → Y)

Both solvers implement `IPolynomialFitService` (`GetValues(IEnumerable<DataTwoFact>)`), take an
`IBasisExponents` (`SecondOrderBasisExponents` / `ThirdOrderBasisExponents` /
`FourthOrderBasisExponents`, 9 / 16 / 25 coefficients) and return the coefficients of the
original monomials X1^i · X2^j in that basis's fixed order, so they are interchangeable:

```csharp
Func<List<DataTwoFact>, IEnumerable<double>> leastSquares =
    data => new PolynomialLeastSquaresSolver(new ThirdOrderBasisExponents()).GetValues(data);
Func<List<DataTwoFact>, IEnumerable<double>> minimax =
    data => new MinimaxPolynomialSolver(new ThirdOrderBasisExponents()).GetValues(data);
```

| Solver | Minimizes | Use when |
|---|---|---|
| `PolynomialLeastSquaresSolver` (also `ILeastSquaresRegressionService`) | sum of squared errors | **Default.** Accuracy matters between calibration temperatures, or the data may contain noisy points. |
| `MinimaxPolynomialSolver` | maximum absolute error over the given points | Acceptance is a max-error bound checked strictly at the calibration points. |

`MinimaxPolynomialSolver` is iterative (Lawson's reweighted least squares, `maxIterations` default
1000) and lands within a few percent of the exact minimax optimum. It lowers the in-sample max error
(third order on the 223/224 datasets: 0.00152 → 0.00095 and 0.00144 → 0.00104), but it chases the
worst points: a noisy point pulls the whole fit toward itself, and it predicts unseen temperatures
worse than least squares (leave-one-series-out, worst interior series: 0.0019 → 0.0029 and
0.0032 → 0.0048).

For either solver, the number of distinct temperatures must exceed the temperature degree of the
basis: at least 3 / 4 / 5 for second / third / fourth order. Otherwise the high-power terms are
undetermined, and the in-sample error will not show it.

## Detecting mis-loaded points in one isothermal series

`IsothermalSeriesOutlierDetector.GetSuspiciousPoints(IEnumerable<DataTwoFact>)` checks **one
series recorded at one temperature** right after it is measured, so that a point loaded with the
wrong dead weight can be re-measured while the sensor is still in the thermostat.

```csharp
var detector = new IsothermalSeriesOutlierDetector(); // relativeTolerance = 5e-5
IReadOnlyList<SuspiciousPoint> suspicious = detector.GetSuspiciousPoints(series);
```

- **Input:** `X1` = pressure code, `X2` = temperature code (not used), `Y` = target pressure the
  loads are meant to reproduce. All `Y` of the series must be on one scale (e.g. gauge pressure,
  vacuum negative); bringing points recorded on different reference gauges to one scale is the
  caller's job. Units don't matter.
- **Model:** a loading mistake reproduces a different pressure, so it shows up as a wrong
  pressure code at an unchanged `Y`. The detector fits the characteristic `X1 = g(Y)`
  (2nd degree) and looks at the code residuals.
- **Empty list:** the series is clean.
- **All `Outlier`:** the smallest set of points whose removal leaves the rest on a smooth
  2nd-degree curve is unique. `CodeError` = `X1 − g(Y)`: how far the recorded pressure code is
  from the code the clean characteristic gives at the target pressure, in `X1` units.
- **All `AmbiguousPoint`:** several sets fit equally well; re-check every returned point.
- **`SeriesNotResolvableException`:** more than `MaxOutliers` (at most 4) points are off, or the
  threshold is wrong. Re-measure the series. `Tolerance` is the τ that was used, in codes.

**Threshold.** τ = `relativeTolerance` × (Ymax − Ymin) × |s|, where s is the Theil–Sen slope of
`X1` over `Y` (the median of the pairwise slopes). The scale comes from `Y` because `Y` is exact,
while `X1` contains the very points being searched: a gross error at an end of the series would
change the code range and with it τ, but it barely moves the median slope. On clean data
(Ymax − Ymin)·|s| equals the code range to within 0.02%. τ is the **sensor noise** in codes, not
half a weight: with a τ that large the curve absorbs an error at an end point.

**Margin.** The worst clean residual, max |X1 − g(Y)| / ((Ymax − Ymin)·|s|), measured on 15 real
series: sensor 235 0.23–1.29·10⁻⁵, 223 1.85–2.22·10⁻⁵, 224 1.73–2.59·10⁻⁵. The default 5·10⁻⁵ is
1.9× the worst of them (224, series 5).

**Tested scope.** All 15 real series of 3 sensors: each clean series; 1 error at every position
0–10 (±0.05 and ±0.15 codes); 2 errors at 6 position pairs (both ends, neighbours, end + neighbour,
spread apart); 6 errors → exception. Up to 4 errors are supported by
the algorithm, but more than 2 are deliberately not tested, because an operator rarely makes more
than two loading mistakes in one series.

**Points the others can't verify.** On an unevenly spaced series, a point far from the others
(e.g. the last point on another reference gauge, or a single deep vacuum point) has leverage close
to 1 in the 2nd-degree fit: the curve passes through it whatever its value, so the rest of the
series cannot check it. If any point kept in the fit has leverage > 0.9, the whole result becomes
`AmbiguousPoint`, and that point is always among them, even for an otherwise clean series. If
the isolated point is itself the removed outlier, the `Outlier` result stands. On the real series
the largest leverage of a kept point is 0.78 (235) and 0.79 (223, 224), across the clean, 1-error and
2-error tests.

**Synthetic tests.** Four hand-built unevenly spaced samples (mildly uneven, vacuum with a gap,
isolated end point, isolated start point) are tested separately from the real data: 0, 1 and 2
errors. On the isolated samples, the result is always safe: either exactly the injected
`Outlier`s, or `AmbiguousPoint`s that include every injected point.

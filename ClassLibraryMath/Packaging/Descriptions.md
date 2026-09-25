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

using Regression.Two_factor_regression.Implements;
using Regression.ErrorAnalysis;

namespace SolversTests;

/// <summary>
/// Same sensor 235 "FiveOriginal" dataset as ThirdOrder/SeriesCountAnalysis/FiveOriginal.cs
/// (5 real temperature series, 55 points), fit with a fourth-order (25-coefficient, full 5x5
/// grid) two-factor polynomial instead of the third-order (16-coefficient) one, to test
/// whether a higher-order model closes the tolerance gap described in issue #10.
///
/// PASSES: GetMax() = 0.0023205898284857085, under the 0.003 tolerance (the third-order model
/// on the same dataset gets 0.0067532704376560559 — see ThirdOrder/SeriesCountAnalysis/
/// FiveOriginal.cs). Going to fourth order closes the gap for this dataset; see issue #10 for
/// whether that's the right fix or just curve-fits around a conditioning issue.
/// </summary>
public class FiveOriginalFourthOrder
{
    [Fact]
    public void MaxErrorIsBelowTolerance()
    {
        var sut = new PolynomialLeastSquaresSolver(new FourthOrderBasisExponents());
        var coefficients = sut.GetValues(SeriesCountAnalysisData.FiveOriginal);
        var error = new ApproximationCalculationError(coefficients, SeriesCountAnalysisData.FiveOriginal).GetMax();

        Assert.True(error < 0.003);
    }
}

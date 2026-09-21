using Regression.Two_factor_regression.Implements;
using Regression.ErrorAnalysis;

namespace SolversTests;

/// <summary>
/// Sensor 235 calibration data, "ThreeOriginal" strategy: 3 real temperature series (33
/// points), all measured. See SeriesCountAnalysis/FiveOriginal.cs and
/// ThreeOriginalAndTwoFake.cs for the other two data-collection strategies being compared
/// against this one, and the PR description for the comparative analysis.
///
/// GetMax() = 0.0012097178459100633, comfortably below the 0.003 tolerance.
/// </summary>
public class ThreeOriginal
{
    [Fact]
    public void MaxErrorIsBelowTolerance()
    {
        var sut = new PolynomialLeastSquaresSolver(new ThirdOrderBasisExponents());
        var coefficients = sut.GetValues(SeriesCountAnalysisData.ThreeOriginal);
        var error = new ApproximationCalculationError(coefficients, SeriesCountAnalysisData.ThreeOriginal).GetMax();

        Assert.True(error < 0.003);
    }
}

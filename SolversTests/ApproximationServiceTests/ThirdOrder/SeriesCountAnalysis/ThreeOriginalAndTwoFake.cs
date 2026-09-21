using Regression.Two_factor_regression.Implements;
using Regression.ErrorAnalysis;

namespace SolversTests;

/// <summary>
/// Sensor 235 calibration data, "ThreeOriginalAndTwoFake" strategy: 5 series (55 points), but
/// only the same 3 real series as ThreeOriginal.cs were measured — the other 2 were generated
/// by fitting a 2nd-degree polynomial rather than measured. See ThreeOriginal.cs and
/// FiveOriginal.cs for the other two data-collection strategies being compared against this
/// one, and the PR description for the comparative analysis.
///
/// GetMax() = 0.0012323117431662922, comfortably below the 0.003 tolerance — essentially
/// identical to ThreeOriginal.cs (0.0012097178459100633), and far below FiveOriginal.cs's
/// 0.0067532704376560559 (which fails the tolerance). See the PR description.
/// </summary>
public class ThreeOriginalAndTwoFake
{
    [Fact]
    public void MaxErrorIsBelowTolerance()
    {
        var sut = new PolynomialLeastSquaresSolver(new ThirdOrderBasisExponents());
        var coefficients = sut.GetValues(SeriesCountAnalysisData.ThreeOriginalAndTwoFake);
        var error = new ApproximationCalculationError(coefficients, SeriesCountAnalysisData.ThreeOriginalAndTwoFake).GetMax();

        Assert.True(error < 0.003);
    }
}

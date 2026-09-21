using Regression.Two_factor_regression.Implements;
using Regression.ErrorAnalysis;

namespace SolversTests;

/// <summary>
/// Sensor 235 calibration data, "FiveOriginal" strategy: 5 real temperature series (55
/// points), a superset of ThreeOriginal.cs plus 2 more real series. See ThreeOriginal.cs and
/// ThreeOriginalAndTwoFake.cs for the other two data-collection strategies being compared
/// against this one, and the PR description for the comparative analysis.
///
/// KNOWN FAILING: GetMax() = 0.0067532704376560559, above the 0.003 tolerance (ThreeOriginal
/// = 0.0012097178459100633 and ThreeOriginalAndTwoFake = 0.0012323117431662922 both pass it
/// comfortably). This is intentionally left red rather than loosened or excluded — see the PR
/// description for the comparative analysis and issue #10 tracking it as a distinct, real
/// model-fit-quality gap (not a conditioning/solver bug like #5-#7, which this dataset
/// exercises correctly). See also FiveOriginalDegree3And1.cs / FiveOriginalDegree3And2.cs for
/// the reduced-T-degree experiment that followed up on this finding.
/// </summary>
public class FiveOriginal
{
    [Fact]
    public void MaxErrorIsBelowTolerance()
    {
        var sut = new PolynomialLeastSquaresSolver(new ThirdOrderBasisExponents());
        var coefficients = sut.GetValues(SeriesCountAnalysisData.FiveOriginal);
        var error = new ApproximationCalculationError(coefficients, SeriesCountAnalysisData.FiveOriginal).GetMax();

        Assert.True(error < 0.003);
    }
}

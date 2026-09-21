using Regression.Two_factor_regression.Implements;
using Regression.Two_factor_regression;
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
    private List<DataTwoFact> _data = new()
    {
        new DataTwoFact() { X1 = -1.05452, X2 = -709.6729, Y = 0 },
        new DataTwoFact() { X1 = -7.02454, X2 = -709.6721, Y = 19.6239 },
        new DataTwoFact() { X1 = -12.99435, X2 = -709.6715, Y = 39.2488 },
        new DataTwoFact() { X1 = -19.26239, X2 = -709.6706, Y = 59.8547 },
        new DataTwoFact() { X1 = -25.38106, X2 = -709.67, Y = 79.9702 },
        new DataTwoFact() { X1 = -31.3501, X2 = -709.6695, Y = 99.5958 },
        new DataTwoFact() { X1 = -37.31866, X2 = -709.6696, Y = 119.2207 },
        new DataTwoFact() { X1 = -43.58517, X2 = -709.6696, Y = 139.8271 },
        new DataTwoFact() { X1 = -49.553, X2 = -709.6697, Y = 159.452 },
        new DataTwoFact() { X1 = -55.52017, X2 = -709.6699, Y = 179.0764 },
        new DataTwoFact() { X1 = -61.78557, X2 = -709.6705, Y = 199.6824 },
        new DataTwoFact() { X1 = -1.11354, X2 = -710.7041, Y = 0 },
        new DataTwoFact() { X1 = -7.0831, X2 = -710.703, Y = 19.6239 },
        new DataTwoFact() { X1 = -13.05277, X2 = -710.7021, Y = 39.2488 },
        new DataTwoFact() { X1 = -19.32067, X2 = -710.7012, Y = 59.8547 },
        new DataTwoFact() { X1 = -25.43872, X2 = -710.7006, Y = 79.9702 },
        new DataTwoFact() { X1 = -31.40758, X2 = -710.7002, Y = 99.5958 },
        new DataTwoFact() { X1 = -37.37575, X2 = -710.7, Y = 119.2207 },
        new DataTwoFact() { X1 = -43.64213, X2 = -710.6997, Y = 139.8271 },
        new DataTwoFact() { X1 = -49.60935, X2 = -710.6998, Y = 159.452 },
        new DataTwoFact() { X1 = -55.57663, X2 = -710.7, Y = 179.0764 },
        new DataTwoFact() { X1 = -61.84075, X2 = -710.7005, Y = 199.6824 },
        new DataTwoFact() { X1 = -1.18305, X2 = -711.8979, Y = 0 },
        new DataTwoFact() { X1 = -7.15069, X2 = -711.897, Y = 19.6239 },
        new DataTwoFact() { X1 = -13.11827, X2 = -711.896, Y = 39.2488 },
        new DataTwoFact() { X1 = -19.38371, X2 = -711.8955, Y = 59.8547 },
        new DataTwoFact() { X1 = -25.50032, X2 = -711.8949, Y = 79.9702 },
        new DataTwoFact() { X1 = -31.46735, X2 = -711.8945, Y = 99.5958 },
        new DataTwoFact() { X1 = -37.43331, X2 = -711.8942, Y = 119.2207 },
        new DataTwoFact() { X1 = -43.69813, X2 = -711.894, Y = 139.8271 },
        new DataTwoFact() { X1 = -49.66398, X2 = -711.8942, Y = 159.452 },
        new DataTwoFact() { X1 = -55.62907, X2 = -711.8944, Y = 179.0764 },
        new DataTwoFact() { X1 = -61.89198, X2 = -711.895, Y = 199.6824 },
    };

    [Fact]
    public void MaxErrorIsBelowTolerance()
    {
        var sut = new PolynomialLeastSquaresSolver(new ThirdOrderBasisExponents());
        var coefficients = sut.GetValues(_data);
        var error = new ApproximationCalculationError(coefficients, _data).GetMax();

        Assert.True(error < 0.003);
    }
}

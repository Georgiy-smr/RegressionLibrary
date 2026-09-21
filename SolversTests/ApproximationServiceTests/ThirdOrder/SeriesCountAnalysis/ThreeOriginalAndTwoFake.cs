using Regression.Two_factor_regression.Implements;
using Regression.Two_factor_regression;
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
        new DataTwoFact() { X1 = -1.08396538461545, X2 = -710.189416483515, Y = 0 },
        new DataTwoFact() { X1 = -7.05389219780244, X2 = -710.188412087907, Y = 19.6239 },
        new DataTwoFact() { X1 = -13.023822747253, X2 = -710.187637362646, Y = 39.2488 },
        new DataTwoFact() { X1 = -19.292019890111, X2 = -710.186697802177, Y = 59.8547 },
        new DataTwoFact() { X1 = -25.4104507692322, X2 = -710.186097802186, Y = 79.9702 },
        new DataTwoFact() { X1 = -31.3795609890119, X2 = -710.185659340671, Y = 99.5958 },
        new DataTwoFact() { X1 = -37.3480995604389, X2 = -710.185584615417, Y = 119.2207 },
        new DataTwoFact() { X1 = -43.6146838461561, X2 = -710.1853901099, Y = 139.8271 },
        new DataTwoFact() { X1 = -49.5822739560465, X2 = -710.185480219862, Y = 159.452 },
        new DataTwoFact() { X1 = -55.5497282417634, X2 = -710.185680219722, Y = 179.0764 },
        new DataTwoFact() { X1 = -61.814460219782, X2 = -710.186208791245, Y = 199.6824 },
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
        new DataTwoFact() { X1 = -1.14820705128217, X2 = -711.302247435897, Y = 0 },
        new DataTwoFact() { X1 = -7.11699326923124, X2 = -711.301173397436, Y = 19.6239 },
        new DataTwoFact() { X1 = -13.0858776282055, X2 = -711.30018974361, Y = 39.2488 },
        new DataTwoFact() { X1 = -19.3528567948739, X2 = -711.299435897401, Y = 59.8547 },
        new DataTwoFact() { X1 = -25.4702832692338, X2 = -711.298835897412, Y = 79.9702 },
        new DataTwoFact() { X1 = -31.4384463461553, X2 = -711.298451602585, Y = 99.5958 },
        new DataTwoFact() { X1 = -37.4057475961527, X2 = -711.298167948778, Y = 119.2207 },
        new DataTwoFact() { X1 = -43.6715371794912, X2 = -711.297857371809, Y = 139.8271 },
        new DataTwoFact() { X1 = -49.638160801287, X2 = -711.297993910408, Y = 159.452 },
        new DataTwoFact() { X1 = -55.6046578846247, X2 = -711.298193910153, Y = 179.0764 },
        new DataTwoFact() { X1 = -61.868134743593, X2 = -711.298714743654, Y = 199.6824 },
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

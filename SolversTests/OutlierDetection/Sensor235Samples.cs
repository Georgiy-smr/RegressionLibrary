using Regression.Two_factor_regression;

namespace SolversTests;

/// <summary>
/// Sensor 235 (0–200 kPa): the 5 real isothermal series of
/// <see cref="SeriesCountAnalysisData.FiveOriginal"/>, written out as 5 separate samples of
/// 11 points each. X1 = pressure code, X2 = temperature code, Y = reference pressure.
/// </summary>
public static class Sensor235Samples
{
    /// <summary>15 °C.</summary>
    public static readonly DataTwoFact[] At15C =
    {
        new DataTwoFact { X1 = -1.05452, X2 = -709.6729, Y = 0 },
        new DataTwoFact { X1 = -7.02454, X2 = -709.6721, Y = 19.6239 },
        new DataTwoFact { X1 = -12.99435, X2 = -709.6715, Y = 39.2488 },
        new DataTwoFact { X1 = -19.26239, X2 = -709.6706, Y = 59.8547 },
        new DataTwoFact { X1 = -25.38106, X2 = -709.67, Y = 79.9702 },
        new DataTwoFact { X1 = -31.3501, X2 = -709.6695, Y = 99.5958 },
        new DataTwoFact { X1 = -37.31866, X2 = -709.6696, Y = 119.2207 },
        new DataTwoFact { X1 = -43.58517, X2 = -709.6696, Y = 139.8271 },
        new DataTwoFact { X1 = -49.553, X2 = -709.6697, Y = 159.452 },
        new DataTwoFact { X1 = -55.52017, X2 = -709.6699, Y = 179.0764 },
        new DataTwoFact { X1 = -61.78557, X2 = -709.6705, Y = 199.6824 },
    };

    /// <summary>18 °C.</summary>
    public static readonly DataTwoFact[] At18C =
    {
        new DataTwoFact { X1 = -1.08806, X2 = -710.1902, Y = 0 },
        new DataTwoFact { X1 = -7.05772, X2 = -710.1895, Y = 19.6239 },
        new DataTwoFact { X1 = -13.02831, X2 = -710.1885, Y = 39.2488 },
        new DataTwoFact { X1 = -19.2957, X2 = -710.1875, Y = 59.8547 },
        new DataTwoFact { X1 = -25.41529, X2 = -710.1873, Y = 79.9702 },
        new DataTwoFact { X1 = -31.38338, X2 = -710.187, Y = 99.5958 },
        new DataTwoFact { X1 = -37.35152, X2 = -710.1866, Y = 119.2207 },
        new DataTwoFact { X1 = -43.61821, X2 = -710.1865, Y = 139.8271 },
        new DataTwoFact { X1 = -49.58601, X2 = -710.1864, Y = 159.452 },
        new DataTwoFact { X1 = -55.55257, X2 = -710.1864, Y = 179.0764 },
        new DataTwoFact { X1 = -61.81788, X2 = -710.1867, Y = 199.6824 },
    };

    /// <summary>21 °C.</summary>
    public static readonly DataTwoFact[] At21C =
    {
        new DataTwoFact { X1 = -1.11354, X2 = -710.7041, Y = 0 },
        new DataTwoFact { X1 = -7.0831, X2 = -710.703, Y = 19.6239 },
        new DataTwoFact { X1 = -13.05277, X2 = -710.7021, Y = 39.2488 },
        new DataTwoFact { X1 = -19.32067, X2 = -710.7012, Y = 59.8547 },
        new DataTwoFact { X1 = -25.43872, X2 = -710.7006, Y = 79.9702 },
        new DataTwoFact { X1 = -31.40758, X2 = -710.7002, Y = 99.5958 },
        new DataTwoFact { X1 = -37.37575, X2 = -710.7, Y = 119.2207 },
        new DataTwoFact { X1 = -43.64213, X2 = -710.6997, Y = 139.8271 },
        new DataTwoFact { X1 = -49.60935, X2 = -710.6998, Y = 159.452 },
        new DataTwoFact { X1 = -55.57663, X2 = -710.7, Y = 179.0764 },
        new DataTwoFact { X1 = -61.84075, X2 = -710.7005, Y = 199.6824 },
    };

    /// <summary>24.5 °C.</summary>
    public static readonly DataTwoFact[] At24_5C =
    {
        new DataTwoFact { X1 = -1.14896, X2 = -711.3016, Y = 0 },
        new DataTwoFact { X1 = -7.11797, X2 = -711.3007, Y = 19.6239 },
        new DataTwoFact { X1 = -13.08647, X2 = -711.2998, Y = 39.2488 },
        new DataTwoFact { X1 = -19.35378, X2 = -711.2989, Y = 59.8547 },
        new DataTwoFact { X1 = -25.47094, X2 = -711.2987, Y = 79.9702 },
        new DataTwoFact { X1 = -31.43859, X2 = -711.2985, Y = 99.5958 },
        new DataTwoFact { X1 = -37.40654, X2 = -711.2983, Y = 119.2207 },
        new DataTwoFact { X1 = -43.67182, X2 = -711.298, Y = 139.8271 },
        new DataTwoFact { X1 = -49.63836, X2 = -711.2981, Y = 159.452 },
        new DataTwoFact { X1 = -55.60482, X2 = -711.298, Y = 179.0764 },
        new DataTwoFact { X1 = -61.86921, X2 = -711.2987, Y = 199.6824 },
    };

    /// <summary>28 °C.</summary>
    public static readonly DataTwoFact[] At28C =
    {
        new DataTwoFact { X1 = -1.18305, X2 = -711.8979, Y = 0 },
        new DataTwoFact { X1 = -7.15069, X2 = -711.897, Y = 19.6239 },
        new DataTwoFact { X1 = -13.11827, X2 = -711.896, Y = 39.2488 },
        new DataTwoFact { X1 = -19.38371, X2 = -711.8955, Y = 59.8547 },
        new DataTwoFact { X1 = -25.50032, X2 = -711.8949, Y = 79.9702 },
        new DataTwoFact { X1 = -31.46735, X2 = -711.8945, Y = 99.5958 },
        new DataTwoFact { X1 = -37.43331, X2 = -711.8942, Y = 119.2207 },
        new DataTwoFact { X1 = -43.69813, X2 = -711.894, Y = 139.8271 },
        new DataTwoFact { X1 = -49.66398, X2 = -711.8942, Y = 159.452 },
        new DataTwoFact { X1 = -55.62907, X2 = -711.8944, Y = 179.0764 },
        new DataTwoFact { X1 = -61.89198, X2 = -711.895, Y = 199.6824 },
    };

    /// <summary>All 5 samples, coldest first; the index is what the tests pass as "sample".</summary>
    public static readonly DataTwoFact[][] All = { At15C, At18C, At21C, At24_5C, At28C };
}

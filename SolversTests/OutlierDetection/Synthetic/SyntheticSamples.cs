using Regression.Two_factor_regression;

namespace SolversTests.Synthetic;

public static class SyntheticSamples
{
    public static readonly DataTwoFact[] MildlyUneven =
    {
        new DataTwoFact { X1 = -1.05429, X2 = -709.6729, Y = 0 },
        new DataTwoFact { X1 = -3.86340, X2 = -709.6721, Y = 9.2324 },
        new DataTwoFact { X1 = -7.57446, X2 = -709.6715, Y = 21.432 },
        new DataTwoFact { X1 = -12.89185, X2 = -709.6706, Y = 38.9107 },
        new DataTwoFact { X1 = -19.83682, X2 = -709.67, Y = 61.7433 },
        new DataTwoFact { X1 = -25.48171, X2 = -709.6695, Y = 80.3021 },
        new DataTwoFact { X1 = -30.90668, X2 = -709.6696, Y = 98.1377 },
        new DataTwoFact { X1 = -38.05486, X2 = -709.6696, Y = 121.6412 },
        new DataTwoFact { X1 = -43.70473, X2 = -709.6697, Y = 140.2208 },
        new DataTwoFact { X1 = -50.58174, X2 = -709.6699, Y = 162.8349 },
        new DataTwoFact { X1 = -61.78541, X2 = -709.6705, Y = 199.6824 },
    };

    public static readonly DataTwoFact[] VacuumWithGap =
    {
        new DataTwoFact { X1 = 17.27120, X2 = -709.6729, Y = -60.2311 },
        new DataTwoFact { X1 = 11.14728, X2 = -709.6721, Y = -40.1057 },
        new DataTwoFact { X1 = 5.12530, X2 = -709.6715, Y = -20.3122 },
        new DataTwoFact { X1 = -1.05475, X2 = -709.6706, Y = 0 },
        new DataTwoFact { X1 = -13.72415, X2 = -709.67, Y = 41.648 },
        new DataTwoFact { X1 = -25.66731, X2 = -709.6695, Y = 80.9123 },
        new DataTwoFact { X1 = -31.84858, X2 = -709.6696, Y = 101.2346 },
        new DataTwoFact { X1 = -43.54718, X2 = -709.6696, Y = 139.7015 },
        new DataTwoFact { X1 = -49.82104, X2 = -709.6697, Y = 160.3342 },
        new DataTwoFact { X1 = -56.07846, X2 = -709.6699, Y = 180.9121 },
        new DataTwoFact { X1 = -61.78541, X2 = -709.6705, Y = 199.6824 },
    };

    public static readonly DataTwoFact[] IsolatedEnd =
    {
        new DataTwoFact { X1 = -1.05429, X2 = -709.6729, Y = 0 },
        new DataTwoFact { X1 = -5.68229, X2 = -709.6721, Y = 15.2113 },
        new DataTwoFact { X1 = -10.22507, X2 = -709.6715, Y = 30.1452 },
        new DataTwoFact { X1 = -14.83607, X2 = -709.6706, Y = 45.3021 },
        new DataTwoFact { X1 = -19.44090, X2 = -709.67, Y = 60.4417 },
        new DataTwoFact { X1 = -23.90823, X2 = -709.6695, Y = 75.1289 },
        new DataTwoFact { X1 = -28.50254, X2 = -709.6696, Y = 90.2331 },
        new DataTwoFact { X1 = -33.08558, X2 = -709.6696, Y = 105.3017 },
        new DataTwoFact { X1 = -37.58955, X2 = -709.6697, Y = 120.1124 },
        new DataTwoFact { X1 = -42.25123, X2 = -709.6699, Y = 135.4402 },
        new DataTwoFact { X1 = -61.78541, X2 = -709.6705, Y = 199.6824 },
    };

    public static readonly DataTwoFact[] IsolatedStart =
    {
        new DataTwoFact { X1 = 44.68976, X2 = -709.6729, Y = -150.3317 },
        new DataTwoFact { X1 = -1.05467, X2 = -709.6721, Y = 0 },
        new DataTwoFact { X1 = -7.17299, X2 = -709.6715, Y = 20.1123 },
        new DataTwoFact { X1 = -13.29138, X2 = -709.6706, Y = 40.2241 },
        new DataTwoFact { X1 = -19.33657, X2 = -709.67, Y = 60.0987 },
        new DataTwoFact { X1 = -25.49056, X2 = -709.6695, Y = 80.3312 },
        new DataTwoFact { X1 = -31.50791, X2 = -709.6696, Y = 100.1145 },
        new DataTwoFact { X1 = -37.62268, X2 = -709.6696, Y = 120.2201 },
        new DataTwoFact { X1 = -43.74031, X2 = -709.6697, Y = 140.3378 },
        new DataTwoFact { X1 = -49.75321, X2 = -709.6699, Y = 160.1102 },
        new DataTwoFact { X1 = -55.93569, X2 = -709.6705, Y = 180.4431 },
    };
}

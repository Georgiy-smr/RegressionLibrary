using Regression.Two_factor_regression;

namespace SolversTests;

public static class Sensor223And224Data
{
    public static readonly List<DataTwoFact> Data223 = Sensor223Samples.All.SelectMany(series => series).ToList();

    public static readonly List<DataTwoFact> Data224 = Sensor224Samples.All.SelectMany(series => series).ToList();
}

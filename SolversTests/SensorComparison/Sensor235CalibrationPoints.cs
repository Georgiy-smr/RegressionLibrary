using Regression.OutlierDetection;
using Regression.Two_factor_regression;

namespace SolversTests.SensorComparison;

public static class Sensor235CalibrationPoints
{
    private static readonly (double Temperature, DataTwoFact[] Sample)[] Samples =
    {
        (15, Sensor235Samples.At15C),
        (18, Sensor235Samples.At18C),
        (21, Sensor235Samples.At21C),
        (24.5, Sensor235Samples.At24_5C),
        (28, Sensor235Samples.At28C),
    };

    public static readonly CalibrationPoint[] All = Series(15, 18, 21, 24.5, 28);

    public static CalibrationPoint[] Series(params double[] temperatures)
        => Samples
            .Where(series => temperatures.Contains(series.Temperature))
            .SelectMany(series => series.Sample.Select(point => new CalibrationPoint(point.X1, point.X2, point.Y, series.Temperature)))
            .ToArray();
}

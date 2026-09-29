using Regression.OutlierDetection;
using Regression.Two_factor_regression;

namespace SolversTests.Dataset;

public static class Sensor235Dataset
{
    public static readonly double[] TemperatureJitter = { 0.03, -0.02, 0.01, -0.04, 0.00, 0.02, -0.01, 0.04, -0.03, 0.01, -0.01 };

    public static readonly (double Temperature, DataTwoFact[] Sample)[] Series =
    {
        (15, Sensor235Samples.At15C),
        (18, Sensor235Samples.At18C),
        (21, Sensor235Samples.At21C),
        (24.5, Sensor235Samples.At24_5C),
        (28, Sensor235Samples.At28C),
    };

    public static CalibrationPoint[] Build()
        => Series
            .SelectMany(series => series.Sample.Select((point, index) =>
                new CalibrationPoint(point.X1, point.X2, point.Y, series.Temperature + TemperatureJitter[index])))
            .ToArray();

    public static CalibrationPoint[] WithCodeErrors(CalibrationPoint[] dataset, params int[] rows)
    {
        var points = dataset.ToArray();
        foreach (var row in rows)
            points[row] = points[row] with { PressureCode = points[row].PressureCode + 0.15 };
        return points;
    }
}

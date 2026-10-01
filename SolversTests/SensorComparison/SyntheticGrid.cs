using Regression.OutlierDetection;

namespace SolversTests.SensorComparison;

public static class SyntheticGrid
{
    public static double TemperatureCode(double temperature) => 970000 - 2100 * temperature;

    public static CalibrationPoint[] Build(double[] pressures, double[] temperatures, Func<double, double, double> pressureCode)
        => temperatures
            .SelectMany(temperature => pressures.Select(pressure =>
                new CalibrationPoint(pressureCode(pressure, temperature), TemperatureCode(temperature), pressure, temperature)))
            .ToArray();
}

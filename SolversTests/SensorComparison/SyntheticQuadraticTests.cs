using Regression.SectorCompensation;

namespace SolversTests.SensorComparison;

public class SyntheticQuadraticTests
{
    private static readonly double[] Pressures = { 0, 30, 60, 90, 120, 150, 180, 210, 240, 270, 300 };
    private static readonly double[] Temperatures = { 15, 18, 21, 24, 27 };

    private readonly SectorSensor _sensor = new SectorSensorCharacterizer(new PolynomialSensorCharacterizer())
        .Characterize(SyntheticGrid.Build(Pressures, Temperatures, PressureCode));

    [Theory]
    [InlineData(45, 16.5)]
    [InlineData(100, 20)]
    [InlineData(135, 22.3)]
    [InlineData(215, 25.3)]
    [InlineData(10, 26.9)]
    [InlineData(299, 15.2)]
    public void PointBetweenNodes(double pressure, double temperature)
    {
        var actual = _sensor.GetPressure(PressureCode(pressure, temperature), SyntheticGrid.TemperatureCode(temperature));

        Assert.InRange(Math.Abs(actual - pressure), 0, 1e-9);
    }

    private static double PressureCode(double pressure, double temperature)
        => 250000 + 17900 * pressure - 5800 * temperature - 0.35 * pressure * pressure + 1.2 * temperature * temperature - 37 * pressure * temperature;
}

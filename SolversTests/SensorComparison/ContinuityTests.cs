using Regression.OutlierDetection;
using Regression.SectorCompensation;

namespace SolversTests.SensorComparison;

public class ContinuityTests
{
    private const double Offset = 1e-6;

    private static readonly double[] Positions = { 0, 30, 60, 90, 120, 150, 180, 210, 240, 270, 300 };
    private static readonly double[] Temperatures = { 15, 18, 21, 24, 27 };

    private readonly SectorSensor _sensor = new SectorSensorCharacterizer(new PolynomialSensorCharacterizer())
        .Characterize(Temperatures.SelectMany(temperature => Positions.Select(position => Point(position, temperature))));

    [Fact]
    public void HorizontalEdge() => AssertNoJump(100, 21);

    [Fact]
    public void VerticalEdge() => AssertNoJump(120, 16.4);

    [Fact]
    public void Diagonal() => AssertNoJump(20, 15 + 6 * (Pressure(60) - Pressure(20)) / (Pressure(60) - Pressure(0)));

    private void AssertNoJump(double position, double temperature)
    {
        var before = Error(Point(position - Offset, temperature - Offset));
        var after = Error(Point(position + Offset, temperature + Offset));

        Assert.NotEqual(0, before);
        Assert.InRange(Math.Abs(after - before), 0, 1e-6);
    }

    private double Error(CalibrationPoint point) => _sensor.GetPressure(point.PressureCode, point.TemperatureCode) - point.Pressure;

    private static double Pressure(double position) => position + 1e-6 * position * position * position;

    private static CalibrationPoint Point(double position, double temperature)
        => new(17000 * (position + 0.3 * temperature), SyntheticGrid.TemperatureCode(temperature), Pressure(position), temperature);
}

using Regression.SectorCompensation;

namespace SolversTests.SensorComparison;

public class WorkedExampleTests
{
    private readonly SectorSensor _sensor = new SectorSensorCharacterizer(new PolynomialSensorCharacterizer())
        .Characterize(Sensor00249979Dataset.Points);

    [Fact]
    public void PointA() => AssertTrace(2820007, 935670, 150.003851, 19.519809, 6, 150.004397);

    [Fact]
    public void PointB() => AssertTrace(3557300, 946545, 190.005831, 14.976926, 7, 190.007886);

    [Fact]
    public void PointC() => AssertTrace(3352009, 936467, 180.008725, 19.519078, 6, 180.00796985);

    private void AssertTrace(double pressureCode, double temperatureCode, double roughPressure, double roughTemperature, int sectorNumber, double pressure)
    {
        var trace = _sensor.Trace(pressureCode, temperatureCode);

        Assert.InRange(Math.Abs(trace.RoughPressure - roughPressure), 0, 0.0001);
        Assert.InRange(Math.Abs(trace.RoughTemperature - roughTemperature), 0, 0.00001);
        Assert.Equal(sectorNumber, trace.SectorNumber);
        Assert.InRange(Math.Abs(trace.Pressure - pressure), 0, 0.0001);
        Assert.Equal(trace.Pressure, _sensor.GetPressure(pressureCode, temperatureCode));
    }
}

using Regression.SectorCompensation;

namespace SolversTests.SensorComparison;

public class FindSectorTests
{
    private readonly SectorSensor _sensor = new SectorSensorCharacterizer(new PolynomialSensorCharacterizer())
        .Characterize(Sensor00249979Dataset.Points);

    [Fact]
    public void PointA()
    {
        var trace = _sensor.Trace(2820007, 935670);

        Assert.Equal(6, _sensor.FindSector(trace.RoughPressure, trace.RoughTemperature)?.Number);
    }

    [Fact]
    public void PointBIsOutsideTheGrid()
    {
        var trace = _sensor.Trace(3557300, 946545);

        Assert.Null(_sensor.FindSector(trace.RoughPressure, trace.RoughTemperature));
    }

    [Fact]
    public void PointCBeforeRefinement()
    {
        var trace = _sensor.Trace(3352009, 936467);

        Assert.Equal(7, _sensor.FindSector(trace.RoughPressure, trace.RoughTemperature)?.Number);
    }

    [Fact]
    public void PointCAfterRefinement()
    {
        var trace = _sensor.Trace(3352009, 936467);

        Assert.Equal(6, _sensor.FindSector(trace.Pressure, trace.RoughTemperature)?.Number);
    }
}

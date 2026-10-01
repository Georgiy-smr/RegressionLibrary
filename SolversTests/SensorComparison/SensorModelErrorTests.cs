using Regression.OutlierDetection;
using Regression.SectorCompensation;

namespace SolversTests.SensorComparison;

public class SensorModelErrorTests
{
    private static readonly CalibrationPoint[] Points =
    {
        new(10, -0.5, 10, 21),
        new(20, 0.25, 20, 21),
        new(10, 0.25, 10, 15),
        new(20, -0.75, 20, 15),
        new(30, 0.5, 30, 15),
    };

    private readonly SensorModelError _error = new(new OffsetSensor(), Points);

    [Theory]
    [InlineData(0, 0.5)]
    [InlineData(1, 0.25)]
    [InlineData(3, 0.75)]
    public void Current(int index, double expected) => Assert.Equal(expected, _error.GetCurrent(Points[index]));

    [Fact]
    public void Max() => Assert.Equal(0.75, _error.GetMax());

    [Fact]
    public void Mean() => Assert.Equal(0.45, _error.GetMean());

    [Fact]
    public void BySeries()
    {
        var series = _error.GetBySeries();

        Assert.Equal(new[] { new SeriesError(15, 3, 0.75, 0.5), new SeriesError(21, 2, 0.5, 0.375) }, series);
    }

    [Fact]
    public void NullModel() => Assert.Throws<ArgumentNullException>(() => new SensorModelError(null!, Points));

    [Fact]
    public void NullPoints() => Assert.Throws<ArgumentNullException>(() => new SensorModelError(new OffsetSensor(), null!));

    [Fact]
    public void NoPoints() => Assert.Throws<ArgumentException>(() => new SensorModelError(new OffsetSensor(), Array.Empty<CalibrationPoint>()));

    private sealed class OffsetSensor : ISensorModel
    {
        public double GetPressure(double pressureCode, double temperatureCode) => pressureCode + temperatureCode;
    }
}

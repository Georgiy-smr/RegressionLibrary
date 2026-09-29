using Regression.OutlierDetection;

namespace SolversTests.Dataset;

public class ShortSeriesTests
{
    private readonly CalibrationDatasetChecker _checker = new();

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(4)]
    public void StrayPointsAt40C(int strayPoints)
    {
        var stray = Sensor235Samples.At28C
            .Take(strayPoints)
            .Select((point, index) => new CalibrationPoint(point.X1, point.X2, point.Y, 40 + Sensor235Dataset.TemperatureJitter[index]));
        var dataset = Sensor235Dataset.Build().Concat(stray).ToArray();

        var result = _checker.Check(dataset);

        Assert.Equal(6, result.Series.Count);
        var skipped = Assert.IsType<SkippedSeries>(result.Series[5]);
        Assert.Equal(40, skipped.NominalTemperature);
        Assert.Equal(Enumerable.Range(55, strayPoints), skipped.Rows);
        Assert.False(string.IsNullOrWhiteSpace(skipped.Reason));
        Assert.All(result.Series.Take(5), series => Assert.Empty(Assert.IsType<CheckedSeries>(series).SuspiciousPoints));
        Assert.Empty(result.SuspiciousPoints);
        Assert.False(result.IsClean);
    }
}

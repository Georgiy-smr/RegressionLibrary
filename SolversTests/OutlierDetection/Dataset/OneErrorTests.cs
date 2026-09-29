using Regression.OutlierDetection;

namespace SolversTests.Dataset;

public class OneErrorTests
{
    private readonly CalibrationDatasetChecker _checker = new();

    [Theory]
    [InlineData(22, 0)]
    [InlineData(27, 5)]
    [InlineData(32, 10)]
    public void ErrorIn21CSeries(int row, int seriesIndex)
    {
        var result = _checker.Check(Sensor235Dataset.WithCodeErrors(Sensor235Dataset.Build(), row));

        var point = Assert.Single(result.SuspiciousPoints);
        Assert.Equal(row, point.Row);
        Assert.Equal(21, point.NominalTemperature);
        var outlier = Assert.IsType<Outlier>(point.Detail);
        Assert.Equal(seriesIndex, outlier.Index);
        Assert.All(result.Series.Where(series => series.NominalTemperature != 21),
            series => Assert.Empty(Assert.IsType<CheckedSeries>(series).SuspiciousPoints));
    }
}

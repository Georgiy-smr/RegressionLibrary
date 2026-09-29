using Regression.OutlierDetection;

namespace SolversTests.Dataset;

public class TwoSeriesErrorTests
{
    private readonly CalibrationDatasetChecker _checker = new();

    [Theory]
    [InlineData(0, 54)]
    [InlineData(5, 44)]
    [InlineData(10, 50)]
    public void ErrorsIn15CAnd28CSeries(int rowIn15C, int rowIn28C)
    {
        var result = _checker.Check(Sensor235Dataset.WithCodeErrors(Sensor235Dataset.Build(), rowIn28C, rowIn15C));

        Assert.Equal(2, result.SuspiciousPoints.Count);
        Assert.Equal(new[] { rowIn15C, rowIn28C }, result.SuspiciousPoints.Select(point => point.Row));
        Assert.Equal(new double[] { 15, 28 }, result.SuspiciousPoints.Select(point => point.NominalTemperature));
        Assert.All(result.SuspiciousPoints, point => Assert.IsType<Outlier>(point.Detail));
        Assert.False(result.IsClean);
    }
}

using Regression.OutlierDetection;

namespace SolversTests.Dataset;

public class UnresolvableSeriesTests
{
    private static readonly int[] SixErrorsIn18CSeries = { 11, 13, 15, 17, 19, 21 };

    private readonly CalibrationDatasetChecker _checker = new();

    [Theory]
    [InlineData(0, 15)]
    [InlineData(27, 21)]
    [InlineData(50, 28)]
    public void SixErrorsIn18CSeriesAndOneElsewhere(int otherRow, double otherNominalTemperature)
    {
        var dataset = Sensor235Dataset.WithCodeErrors(Sensor235Dataset.Build(), SixErrorsIn18CSeries.Append(otherRow).ToArray());

        var result = _checker.Check(dataset);

        var unresolvable = Assert.IsType<UnresolvableSeries>(Assert.Single(result.Series, series => series.NominalTemperature == 18));
        Assert.Equal(4, unresolvable.MaxOutliers);
        Assert.Equal(Enumerable.Range(11, 11), unresolvable.Rows);
        Assert.All(result.Series.Where(series => series.NominalTemperature != 18), series => Assert.IsType<CheckedSeries>(series));
        var point = Assert.Single(result.SuspiciousPoints);
        Assert.Equal(otherRow, point.Row);
        Assert.Equal(otherNominalTemperature, point.NominalTemperature);
        Assert.False(result.IsClean);
    }
}

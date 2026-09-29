using Regression.OutlierDetection;

namespace SolversTests.Dataset;

public class DatasetValidationTests
{
    [Theory]
    [InlineData(0, 2)]
    [InlineData(-0.01, 2)]
    [InlineData(0.01, 0)]
    [InlineData(0.01, -1)]
    public void NonPositiveAccuracyClassOrGapIsRejected(double accuracyClassPercent, double seriesTemperatureGap)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CalibrationDatasetChecker(accuracyClassPercent, seriesTemperatureGap));
    }

    [Fact]
    public void NullDatasetIsRejected()
    {
        Assert.Throws<ArgumentNullException>(() => new CalibrationDatasetChecker().Check(null!));
    }

    [Fact]
    public void EmptyDatasetIsClean()
    {
        var result = new CalibrationDatasetChecker().Check(Array.Empty<CalibrationPoint>());

        Assert.Empty(result.Series);
        Assert.Empty(result.SuspiciousPoints);
        Assert.True(result.IsClean);
    }
}

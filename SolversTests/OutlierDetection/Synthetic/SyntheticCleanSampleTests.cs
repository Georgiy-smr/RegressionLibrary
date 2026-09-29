using Regression.OutlierDetection;

namespace SolversTests.Synthetic;

public class SyntheticCleanSampleTests
{
    private readonly IsothermalSeriesOutlierDetector _detector = new();

    [Fact]
    public void MildlyUneven()
    {
        Assert.Empty(_detector.GetSuspiciousPoints(SyntheticSamples.MildlyUneven));
    }

    [Fact]
    public void VacuumWithGap()
    {
        Assert.Empty(_detector.GetSuspiciousPoints(SyntheticSamples.VacuumWithGap));
    }

    [Fact]
    public void IsolatedEnd()
    {
        var point = Assert.IsType<AmbiguousPoint>(Assert.Single(_detector.GetSuspiciousPoints(SyntheticSamples.IsolatedEnd)));
        Assert.Equal(10, point.Index);
    }

    [Fact]
    public void IsolatedStart()
    {
        var point = Assert.IsType<AmbiguousPoint>(Assert.Single(_detector.GetSuspiciousPoints(SyntheticSamples.IsolatedStart)));
        Assert.Equal(0, point.Index);
    }
}

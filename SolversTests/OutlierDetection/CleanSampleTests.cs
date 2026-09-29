using Regression.OutlierDetection;
using Regression.Two_factor_regression;

namespace SolversTests;

public class CleanSampleTests
{
    private readonly IsothermalSeriesOutlierDetector _detector = new();

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Sensor235(int series) => AssertClean(Sensor235Samples.All[series]);

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Sensor223(int series) => AssertClean(Sensor223Samples.All[series]);

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Sensor224(int series) => AssertClean(Sensor224Samples.All[series]);

    private void AssertClean(DataTwoFact[] sample)
    {
        Assert.Empty(_detector.GetSuspiciousPoints(sample));
    }
}

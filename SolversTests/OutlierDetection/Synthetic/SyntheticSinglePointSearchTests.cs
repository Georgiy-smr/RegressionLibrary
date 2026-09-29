using Regression.OutlierDetection;
using Regression.Two_factor_regression;

namespace SolversTests.Synthetic;

public class SyntheticSinglePointSearchTests
{
    private const double CodeTolerance = 0.003;

    private readonly IsothermalSeriesOutlierDetector _detector = new();

    [Theory]
    [InlineData(0, 0.15)]
    [InlineData(1, -0.15)]
    [InlineData(2, 0.05)]
    [InlineData(3, -0.05)]
    [InlineData(4, 0.15)]
    [InlineData(5, -0.15)]
    [InlineData(6, 0.05)]
    [InlineData(7, -0.05)]
    [InlineData(8, 0.15)]
    [InlineData(9, -0.15)]
    [InlineData(10, 0.05)]
    public void MildlyUneven(int index, double codeError) => AssertFound(SyntheticSamples.MildlyUneven, index, codeError);

    [Theory]
    [InlineData(0, 0.15)]
    [InlineData(1, -0.15)]
    [InlineData(2, 0.05)]
    [InlineData(3, -0.05)]
    [InlineData(4, 0.15)]
    [InlineData(5, -0.15)]
    [InlineData(6, 0.05)]
    [InlineData(7, -0.05)]
    [InlineData(8, 0.15)]
    [InlineData(9, -0.15)]
    [InlineData(10, 0.05)]
    public void VacuumWithGap(int index, double codeError) => AssertFound(SyntheticSamples.VacuumWithGap, index, codeError);

    [Theory]
    [InlineData(0, 0.15)]
    [InlineData(1, -0.15)]
    [InlineData(2, 0.05)]
    [InlineData(3, -0.05)]
    [InlineData(4, 0.15)]
    [InlineData(5, -0.15)]
    [InlineData(6, 0.05)]
    [InlineData(7, -0.05)]
    [InlineData(8, 0.15)]
    [InlineData(9, -0.15)]
    [InlineData(10, 0.05)]
    public void IsolatedEnd(int index, double codeError) => AssertSafe(SyntheticSamples.IsolatedEnd, index, codeError);

    [Theory]
    [InlineData(0, 0.15)]
    [InlineData(1, -0.15)]
    [InlineData(2, 0.05)]
    [InlineData(3, -0.05)]
    [InlineData(4, 0.15)]
    [InlineData(5, -0.15)]
    [InlineData(6, 0.05)]
    [InlineData(7, -0.05)]
    [InlineData(8, 0.15)]
    [InlineData(9, -0.15)]
    [InlineData(10, 0.05)]
    public void IsolatedStart(int index, double codeError) => AssertSafe(SyntheticSamples.IsolatedStart, index, codeError);

    [Fact]
    public void IsolatedStart_SmallErrorAtIsolatedPointIsNotReportedClean()
    {
        var points = SyntheticSamples.IsolatedStart.ToArray();
        points[0] = points[0] with { X1 = points[0].X1 + 0.03 };

        var result = _detector.GetSuspiciousPoints(points);

        Assert.NotEmpty(result);
        Assert.All(result, point => Assert.IsType<AmbiguousPoint>(point));
        Assert.Contains(result, point => point.Index == 0);
    }

    private void AssertFound(DataTwoFact[] sample, int index, double codeError)
    {
        var points = sample.ToArray();
        points[index] = points[index] with { X1 = points[index].X1 + codeError };

        var result = _detector.GetSuspiciousPoints(points);

        var outlier = Assert.IsType<Outlier>(Assert.Single(result));
        Assert.Equal(index, outlier.Index);
        Assert.Equal(codeError, outlier.CodeError, CodeTolerance);
    }

    private void AssertSafe(DataTwoFact[] sample, int index, double codeError)
    {
        var points = sample.ToArray();
        points[index] = points[index] with { X1 = points[index].X1 + codeError };

        SafeResult.AssertSafe(_detector.GetSuspiciousPoints(points), index);
    }
}

using Regression.OutlierDetection;
using Regression.Two_factor_regression;

namespace SolversTests.Synthetic;

public class SyntheticTwoPointSearchTests
{
    private const double CodeTolerance = 0.003;

    private readonly IsothermalSeriesOutlierDetector _detector = new();

    [Theory]
    [InlineData(0, 0.15, 1, 0.15)]
    [InlineData(0, 0.15, 10, -0.15)]
    [InlineData(2, 0.05, 8, -0.05)]
    [InlineData(3, -0.1, 7, 0.1)]
    [InlineData(4, 0.15, 5, -0.15)]
    [InlineData(9, 0.1, 10, 0.1)]
    public void MildlyUneven(int first, double firstError, int second, double secondError)
        => AssertFound(SyntheticSamples.MildlyUneven, first, firstError, second, secondError);

    [Theory]
    [InlineData(0, 0.15, 1, 0.15)]
    [InlineData(0, 0.15, 10, -0.15)]
    [InlineData(2, 0.05, 8, -0.05)]
    [InlineData(3, -0.1, 7, 0.1)]
    [InlineData(4, 0.15, 5, -0.15)]
    [InlineData(9, 0.1, 10, 0.1)]
    public void VacuumWithGap(int first, double firstError, int second, double secondError)
        => AssertFound(SyntheticSamples.VacuumWithGap, first, firstError, second, secondError);

    [Theory]
    [InlineData(0, 0.15, 1, 0.15)]
    [InlineData(0, 0.15, 10, -0.15)]
    [InlineData(2, 0.05, 8, -0.05)]
    [InlineData(3, -0.1, 7, 0.1)]
    [InlineData(4, 0.15, 5, -0.15)]
    [InlineData(9, 0.1, 10, 0.1)]
    public void IsolatedEnd(int first, double firstError, int second, double secondError)
        => AssertSafe(SyntheticSamples.IsolatedEnd, first, firstError, second, secondError);

    [Theory]
    [InlineData(0, 0.15, 1, 0.15)]
    [InlineData(0, 0.15, 10, -0.15)]
    [InlineData(2, 0.05, 8, -0.05)]
    [InlineData(3, -0.1, 7, 0.1)]
    [InlineData(4, 0.15, 5, -0.15)]
    [InlineData(9, 0.1, 10, 0.1)]
    public void IsolatedStart(int first, double firstError, int second, double secondError)
        => AssertSafe(SyntheticSamples.IsolatedStart, first, firstError, second, secondError);

    private void AssertFound(DataTwoFact[] sample, int first, double firstError, int second, double secondError)
    {
        var points = sample.ToArray();
        points[first] = points[first] with { X1 = points[first].X1 + firstError };
        points[second] = points[second] with { X1 = points[second].X1 + secondError };

        var result = _detector.GetSuspiciousPoints(points);

        Assert.Equal(2, result.Count);
        var firstOutlier = Assert.IsType<Outlier>(result[0]);
        var secondOutlier = Assert.IsType<Outlier>(result[1]);
        Assert.Equal(first, firstOutlier.Index);
        Assert.Equal(firstError, firstOutlier.CodeError, CodeTolerance);
        Assert.Equal(second, secondOutlier.Index);
        Assert.Equal(secondError, secondOutlier.CodeError, CodeTolerance);
    }

    private void AssertSafe(DataTwoFact[] sample, int first, double firstError, int second, double secondError)
    {
        var points = sample.ToArray();
        points[first] = points[first] with { X1 = points[first].X1 + firstError };
        points[second] = points[second] with { X1 = points[second].X1 + secondError };

        SafeResult.AssertSafe(_detector.GetSuspiciousPoints(points), first, second);
    }
}

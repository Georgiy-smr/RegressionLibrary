using Regression.OutlierDetection;

namespace SolversTests;

public class TwoPointSearchTests
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
    public void PointsWithCodeErrorsAreFound(int first, double firstError, int second, double secondError)
    {
        var points = Sensor235Samples.At15C.ToArray();
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
}

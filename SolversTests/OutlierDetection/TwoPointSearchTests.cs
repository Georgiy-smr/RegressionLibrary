using Regression.OutlierDetection;
using Regression.Two_factor_regression;

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
    public void Sensor235At15C(int first, double firstError, int second, double secondError)
        => AssertFound(Sensor235Samples.At15C, first, firstError, second, secondError);

    [Theory]
    [InlineData(0, 0.15, 10, -0.15)]
    [InlineData(4, 0.15, 5, -0.15)]
    [InlineData(0, 0.05, 5, -0.05)]
    public void Sensor223Series1(int first, double firstError, int second, double secondError)
        => AssertFound(Sensor223Samples.Series1, first, firstError, second, secondError);

    [Theory]
    [InlineData(0, 0.15, 10, -0.15)]
    [InlineData(4, 0.15, 5, -0.15)]
    [InlineData(0, 0.05, 5, -0.05)]
    public void Sensor224Series1(int first, double firstError, int second, double secondError)
        => AssertFound(Sensor224Samples.Series1, first, firstError, second, secondError);

    [Fact]
    public void Sensor235At15C_GrossEndErrorDoesNotHideSmallError()
    {
        var points = Sensor235Samples.At15C.ToArray();
        points[10] = points[10] with { X1 = points[10].X1 + 30 };
        points[4] = points[4] with { X1 = points[4].X1 + 0.05 };

        var result = _detector.GetSuspiciousPoints(points);

        Assert.Equal(2, result.Count);
        var small = Assert.IsType<Outlier>(result[0]);
        var gross = Assert.IsType<Outlier>(result[1]);
        Assert.Equal(4, small.Index);
        Assert.Equal(0.05, small.CodeError, CodeTolerance);
        Assert.Equal(10, gross.Index);
    }

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
}

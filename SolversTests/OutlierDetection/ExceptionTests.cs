using Regression.OutlierDetection;

namespace SolversTests;

public class ExceptionTests
{
    private readonly IsothermalSeriesOutlierDetector _detector = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-0.00005)]
    public void NonPositiveRelativeToleranceIsRejected(double relativeTolerance)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new IsothermalSeriesOutlierDetector(relativeTolerance));
    }

    [Fact]
    public void SampleWithFourPointsIsRejected()
    {
        var points = Sensor235Samples.At15C.Take(4);

        Assert.Throws<ArgumentException>(() => _detector.GetSuspiciousPoints(points));
    }

    [Fact]
    public void SampleWithEqualCodesIsRejected()
    {
        var points = Sensor235Samples.At15C.Select(point => point with { X1 = -30 });

        Assert.Throws<ArgumentException>(() => _detector.GetSuspiciousPoints(points));
    }

    [Fact]
    public void SampleWithEqualPressuresIsRejected()
    {
        var points = Sensor235Samples.At15C.Select(point => point with { Y = 100 });

        Assert.Throws<ArgumentException>(() => _detector.GetSuspiciousPoints(points));
    }

    [Theory]
    [InlineData(new[] { 0, 2, 4, 6, 8, 10 })]
    [InlineData(new[] { 1, 3, 5, 7, 9, 10 })]
    public void SampleWithSixCodeErrorsIsNotResolvable(int[] indices)
    {
        var points = Sensor235Samples.At15C.ToArray();
        foreach (var index in indices)
            points[index] = points[index] with { X1 = points[index].X1 + 0.15 };

        var exception = Assert.Throws<SeriesNotResolvableException>(() => _detector.GetSuspiciousPoints(points));

        var codeRange = points.Max(point => point.X1) - points.Min(point => point.X1);
        Assert.Equal(4, exception.MaxOutliers);
        Assert.Equal(5e-5 * codeRange, exception.Tolerance, 1e-12);
    }
}

using Regression.OutlierDetection;
using Regression.Two_factor_regression;

namespace SolversTests;

public class ExceptionTests
{
    private readonly IsothermalSeriesOutlierDetector _detector = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    public void NonPositiveAccuracyClassIsRejected(double accuracyClassPercent)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new IsothermalSeriesOutlierDetector(accuracyClassPercent));
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
        var points = WithCodeErrors(Sensor235Samples.At15C, indices);

        var exception = Assert.Throws<SeriesNotResolvableException>(() => _detector.GetSuspiciousPoints(points));

        var cleanTolerance = CleanTolerance(Sensor235Samples.At15C, 0.01);
        Assert.Equal(4, exception.MaxOutliers);
        Assert.Equal(cleanTolerance, exception.Tolerance, cleanTolerance * 1e-3);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(0.02)]
    [InlineData(0.05)]
    public void ToleranceIsHalfOfAccuracyClass(double accuracyClassPercent)
    {
        var points = WithCodeErrors(Sensor235Samples.At15C, new[] { 0, 2, 4, 6, 8, 10 });
        var detector = new IsothermalSeriesOutlierDetector(accuracyClassPercent);

        var exception = Assert.Throws<SeriesNotResolvableException>(() => detector.GetSuspiciousPoints(points));

        var cleanTolerance = CleanTolerance(Sensor235Samples.At15C, accuracyClassPercent);
        Assert.Equal(cleanTolerance, exception.Tolerance, cleanTolerance * 1e-3);
    }

    private static DataTwoFact[] WithCodeErrors(DataTwoFact[] sample, int[] indices)
    {
        var points = sample.ToArray();
        foreach (var index in indices)
            points[index] = points[index] with { X1 = points[index].X1 + 0.15 };
        return points;
    }

    private static double CleanTolerance(DataTwoFact[] sample, double accuracyClassPercent)
    {
        var slopes = new List<double>();
        for (var i = 0; i < sample.Length; i++)
        for (var j = i + 1; j < sample.Length; j++)
            if (sample[j].Y != sample[i].Y)
                slopes.Add((sample[j].X1 - sample[i].X1) / (sample[j].Y - sample[i].Y));

        slopes.Sort();
        var middle = slopes.Count / 2;
        var slope = slopes.Count % 2 == 1 ? slopes[middle] : (slopes[middle - 1] + slopes[middle]) / 2;
        var pressureRange = sample.Max(point => point.Y) - sample.Min(point => point.Y);
        return accuracyClassPercent / 100 / 2 * pressureRange * Math.Abs(slope);
    }
}

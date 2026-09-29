using Regression.OutlierDetection;

namespace SolversTests;

/// <summary>
/// Searching for two mis-loaded points in a sensor 235 sample of 11 points: after the pressure
/// codes of two points are changed, exactly those two points are found.
/// </summary>
public class TwoPointSearchTests
{
    /// <summary>
    /// How much the pressure code of a spoiled point is changed: 0.15 codes ≈ 0.5 kPa on
    /// sensor 235, ≈ 50τ with the default threshold.
    /// </summary>
    private const double PressureCodeShift = 0.15;

    private readonly IsothermalSeriesOutlierDetector _detector = new();

    /// <summary>Every pair of points of every sample: (sample, first, second).</summary>
    public static IEnumerable<object[]> EveryPair()
        => from sample in Enumerable.Range(0, Sensor235Samples.All.Length)
           let length = Sensor235Samples.All[sample].Length
           from first in Enumerable.Range(0, length)
           from second in Enumerable.Range(first + 1, length - first - 1)
           select new object[] { sample, first, second };

    [Theory]
    [MemberData(nameof(EveryPair))]
    public void PointsWithChangedPressureCodeAreFound(int sample, int first, int second)
    {
        var points = Sensor235Samples.All[sample].ToArray();
        points[first] = points[first] with { X1 = points[first].X1 + PressureCodeShift };
        points[second] = points[second] with { X1 = points[second].X1 + PressureCodeShift };

        var result = _detector.GetSuspiciousPoints(points);

        Assert.All(result, point => Assert.IsType<Outlier>(point));
        Assert.Equal(new[] { first, second }, result.Select(point => point.Index));
    }
}

using Regression.OutlierDetection;

namespace SolversTests;

/// <summary>
/// Searching for one mis-loaded point in a sensor 235 sample of 11 points: a clean sample gives
/// an empty result; after the pressure code of one point is changed, that point is found.
/// </summary>
public class SinglePointSearchTests
{
    /// <summary>
    /// How much the pressure code of a spoiled point is changed: 0.15 codes ≈ 0.5 kPa on
    /// sensor 235, ≈ 50τ with the default threshold.
    /// </summary>
    private const double PressureCodeShift = 0.15;

    private readonly IsothermalSeriesOutlierDetector _detector = new();

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void CleanSampleHasNoSuspiciousPoints(int sample)
    {
        var result = _detector.GetSuspiciousPoints(Sensor235Samples.All[sample]);

        Assert.Empty(result);
    }

    /// <summary>Every point of every sample: (sample, index).</summary>
    public static IEnumerable<object[]> EveryPoint()
        => from sample in Enumerable.Range(0, Sensor235Samples.All.Length)
           from index in Enumerable.Range(0, Sensor235Samples.All[sample].Length)
           select new object[] { sample, index };

    [Theory]
    [MemberData(nameof(EveryPoint))]
    public void PointWithChangedPressureCodeIsFound(int sample, int index)
    {
        var points = Sensor235Samples.All[sample].ToArray();
        points[index] = points[index] with { X1 = points[index].X1 + PressureCodeShift };

        var result = _detector.GetSuspiciousPoints(points);

        var outlier = Assert.IsType<Outlier>(Assert.Single(result));
        Assert.Equal(index, outlier.Index);
    }
}

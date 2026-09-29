using Regression.OutlierDetection;

namespace SolversTests;

/// <summary>
/// Searching for one mis-loaded point in the sensor 235 sample at 15 °C (11 points): a clean
/// sample gives an empty result; after an error is added to the pressure code of one point,
/// that point is found with that same error.
/// </summary>
public class SinglePointSearchTests
{
    /// <summary>
    /// The detector's τ for this sample: 5·10⁻⁵ × code range 60.73 ≈ 0.003 codes.
    /// </summary>
    private const double CodeTolerance = 0.003;

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
    public void PointWithCodeErrorIsFound(int index, double codeError)
    {
        var points = Sensor235Samples.At15C.ToArray();
        points[index] = points[index] with { X1 = points[index].X1 + codeError };

        var result = _detector.GetSuspiciousPoints(points);

        var outlier = Assert.IsType<Outlier>(Assert.Single(result));
        Assert.Equal(index, outlier.Index);
        Assert.Equal(codeError, outlier.CodeError, CodeTolerance);
    }
}

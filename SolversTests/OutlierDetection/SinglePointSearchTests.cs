using Regression.OutlierDetection;
using Regression.Two_factor_regression;

namespace SolversTests;

public class SinglePointSearchTests
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
    public void Sensor235At15C(int index, double codeError) => AssertFound(Sensor235Samples.At15C, index, codeError);

    [Theory]
    [InlineData(0, 0.05)]
    [InlineData(0, -0.15)]
    [InlineData(5, -0.05)]
    [InlineData(5, 0.15)]
    [InlineData(10, 0.05)]
    [InlineData(10, -0.15)]
    public void Sensor223Series1(int index, double codeError) => AssertFound(Sensor223Samples.Series1, index, codeError);

    [Theory]
    [InlineData(0, 0.05)]
    [InlineData(0, -0.15)]
    [InlineData(5, -0.05)]
    [InlineData(5, 0.15)]
    [InlineData(10, 0.05)]
    [InlineData(10, -0.15)]
    public void Sensor224Series1(int index, double codeError) => AssertFound(Sensor224Samples.Series1, index, codeError);

    private void AssertFound(DataTwoFact[] sample, int index, double codeError)
    {
        var points = sample.ToArray();
        points[index] = points[index] with { X1 = points[index].X1 + codeError };

        var result = _detector.GetSuspiciousPoints(points);

        var outlier = Assert.IsType<Outlier>(Assert.Single(result));
        Assert.Equal(index, outlier.Index);
        Assert.Equal(codeError, outlier.CodeError, CodeTolerance);
    }
}

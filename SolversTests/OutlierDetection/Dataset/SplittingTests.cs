using Regression.OutlierDetection;

namespace SolversTests.Dataset;

public class SplittingTests
{
    private static readonly int[] Permutation = Enumerable.Range(0, 55).Select(row => row * 23 % 55).ToArray();

    private readonly CalibrationDatasetChecker _checker = new();

    [Theory]
    [InlineData(0, 15, 0)]
    [InlineData(1, 18, 11)]
    [InlineData(2, 21, 22)]
    [InlineData(3, 25, 33)]
    [InlineData(4, 28, 44)]
    public void CleanDataset(int series, double nominalTemperature, int firstRow)
    {
        var result = _checker.Check(Sensor235Dataset.Build());

        Assert.True(result.IsClean);
        Assert.Equal(5, result.Series.Count);
        var check = Assert.IsType<CheckedSeries>(result.Series[series]);
        Assert.Equal(nominalTemperature, check.NominalTemperature);
        Assert.Equal(Enumerable.Range(firstRow, 11), check.Rows);
        Assert.Empty(check.SuspiciousPoints);
    }

    [Theory]
    [InlineData(0, 15, 0)]
    [InlineData(1, 18, 11)]
    [InlineData(2, 21, 22)]
    [InlineData(3, 25, 33)]
    [InlineData(4, 28, 44)]
    public void ShuffledDataset(int series, double nominalTemperature, int firstRow)
    {
        var dataset = Sensor235Dataset.Build();
        var shuffled = Permutation.Select(row => dataset[row]).ToArray();

        var result = _checker.Check(shuffled);

        Assert.True(result.IsClean);
        Assert.Equal(5, result.Series.Count);
        var check = Assert.IsType<CheckedSeries>(result.Series[series]);
        Assert.Equal(nominalTemperature, check.NominalTemperature);
        Assert.Equal(check.Rows.OrderBy(row => row), check.Rows);
        Assert.Equal(Enumerable.Range(firstRow, 11), check.Rows.Select(row => Permutation[row]).OrderBy(row => row));
    }
}

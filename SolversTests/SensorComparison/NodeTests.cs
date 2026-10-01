using Regression.SectorCompensation;

namespace SolversTests.SensorComparison;

public class NodeTests
{
    private static readonly int[] AllPressures = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    private static readonly int[] FivePressures = { 0, 2, 4, 7, 10 };

    [Fact]
    public void Grid5x11() => AssertExactAtNodes(AllPressures);

    [Fact]
    public void Grid5x5() => AssertExactAtNodes(FivePressures);

    private static void AssertExactAtNodes(int[] nodePressureIndices)
    {
        var sensor = new SectorSensorCharacterizer(new PolynomialSensorCharacterizer(), nodePressureIndices).Characterize(Sensor00249979Dataset.Points);
        var nodes = Sensor00249979Dataset.Points.Where((_, row) => nodePressureIndices.Contains(row % 11));

        foreach (var node in nodes)
            Assert.InRange(Math.Abs(sensor.GetPressure(node.PressureCode, node.TemperatureCode) - node.Pressure), 0, 1e-9);
    }
}

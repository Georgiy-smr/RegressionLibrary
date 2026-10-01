using Regression.SectorCompensation;

namespace SolversTests.SensorComparison;

public class SectorValuesTests
{
    private static readonly int[] AllPressures = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    private static readonly int[] FivePressures = { 0, 2, 4, 7, 10 };

    [Fact]
    public void Grid5x11() => AssertValues(AllPressures, 265);

    [Fact]
    public void Grid5x5() => AssertValues(FivePressures, 121);

    private static void AssertValues(int[] nodePressureIndices, int length)
    {
        var characterizer = new SectorSensorCharacterizer(new PolynomialSensorCharacterizer(), nodePressureIndices);
        var sensor = characterizer.Characterize(Sensor00249979Dataset.Points);

        var values = sensor.Values;

        Assert.Equal(length, values.Count);
        Assert.Equal(sensor.RoughSensor.PressureCoefficients.Concat(sensor.RoughSensor.TemperatureCoefficients), values.Take(25));
        for (var k = 1; k <= sensor.Sectors.Count; k++)
        {
            var sector = sensor.Sectors[k - 1];
            Assert.Equal(
                new[]
                {
                    sector.Vertex1.Pressure, sector.Vertex1.Temperature,
                    sector.Vertex2.Pressure, sector.Vertex2.Temperature,
                    sector.Vertex3.Pressure, sector.Vertex3.Temperature,
                    sector.C0, sector.C1, sector.C2, sector.C3, sector.C4, sector.C5,
                },
                values.Skip(25 + 12 * (k - 1)).Take(12));
        }
        Assert.Equal(values, new SectorFitService(characterizer).GetValues(Sensor00249979Dataset.Points));
    }
}

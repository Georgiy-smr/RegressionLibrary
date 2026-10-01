using Regression.SectorCompensation;

namespace SolversTests.SensorComparison;

public class SectorValuesTests
{
    private static readonly int[] AllPressures = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    private static readonly int[] FivePressures = { 0, 2, 4, 7, 10 };

    [Fact]
    public void SectorValues5x11() => AssertSectorValues(Characterize(AllPressures));

    [Fact]
    public void SectorValues5x5() => AssertSectorValues(Characterize(FivePressures));

    [Fact]
    public void StoredValueCount5x11() => Assert.Equal(265, Characterize(AllPressures).StoredValueCount);

    [Fact]
    public void StoredValueCount5x5() => Assert.Equal(121, Characterize(FivePressures).StoredValueCount);

    [Fact]
    public void RoundTrip5x11() => AssertRoundTrip(Characterize(AllPressures));

    [Fact]
    public void RoundTrip5x5() => AssertRoundTrip(Characterize(FivePressures));

    [Fact]
    public void SectorNumbersOutOfOrder()
    {
        var sensor = Characterize(AllPressures);

        Assert.Throws<ArgumentException>(() => new SectorSensor(sensor.RoughSensor, sensor.Sectors.Reverse()));
        Assert.Throws<ArgumentException>(() => new SectorSensor(sensor.RoughSensor, sensor.Sectors.Skip(1)));
    }

    private static SectorSensor Characterize(int[] nodePressureIndices)
        => new SectorSensorCharacterizer(new PolynomialSensorCharacterizer(), nodePressureIndices).Characterize(Sensor00249979Dataset.Points);

    private static void AssertSectorValues(SectorSensor sensor)
    {
        foreach (var sector in sensor.Sectors)
            Assert.Equal(
                new[]
                {
                    sector.Vertex1.Pressure, sector.Vertex1.Temperature,
                    sector.Vertex2.Pressure, sector.Vertex2.Temperature,
                    sector.Vertex3.Pressure, sector.Vertex3.Temperature,
                    sector.Coefficients[0], sector.Coefficients[1], sector.Coefficients[2],
                    sector.Coefficients[3], sector.Coefficients[4], sector.Coefficients[5],
                },
                sector.Values);
    }

    private static void AssertRoundTrip(SectorSensor sensor)
    {
        var savedPressureCoefficients = sensor.RoughSensor.PressureCoefficients.ToArray();
        var savedTemperatureCoefficients = sensor.RoughSensor.TemperatureCoefficients.ToArray();
        var savedSectors = sensor.Sectors.Select(sector => sector.Values.ToArray()).ToArray();

        var rebuilt = new SectorSensor(
            new PolynomialSensor(savedPressureCoefficients, savedTemperatureCoefficients),
            savedSectors.Select((row, index) => new Sector(
                index + 1,
                new SectorNode(row[0], row[1]),
                new SectorNode(row[2], row[3]),
                new SectorNode(row[4], row[5]),
                row.Skip(6))));

        foreach (var point in Sensor00249979ValidationGrid.Points)
            Assert.Equal(
                sensor.GetPressure(point.PressureCode, point.TemperatureCode),
                rebuilt.GetPressure(point.PressureCode, point.TemperatureCode));
        Assert.Equal(279, Sensor00249979ValidationGrid.Points.Length);
    }
}

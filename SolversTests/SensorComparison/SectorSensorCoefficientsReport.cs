using System.Globalization;
using Regression.SectorCompensation;
using Xunit.Abstractions;

namespace SolversTests.SensorComparison;

public class SectorSensorCoefficientsReport
{
    private readonly ITestOutputHelper _output;

    public SectorSensorCoefficientsReport(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Grid5x11()
    {
        var sensor = new SectorSensorCharacterizer(new PolynomialSensorCharacterizer()).Characterize(Sensor00249979Dataset.Points);

        _output.WriteLine("Pressure\t\tTemperature\t");
        for (var i = 0; i < sensor.RoughSensor.PressureCoefficients.Count; i++)
        {
            var temperature = i < sensor.RoughSensor.TemperatureCoefficients.Count
                ? $"b{i}\t{Number(sensor.RoughSensor.TemperatureCoefficients[i])}"
                : "\t";
            _output.WriteLine($"a{i}\t{Number(sensor.RoughSensor.PressureCoefficients[i])}\t{temperature}");
        }

        _output.WriteLine("");
        _output.WriteLine("N\tP0\tT0\tP1\tT1\tP2\tT2\tP3\tT3\tc0\tc1\tc2\tc3\tc4\tc5");
        foreach (var sector in sensor.Sectors)
            _output.WriteLine(string.Join("\t", new[]
            {
                sector.Anchor.Pressure, sector.Anchor.Temperature,
                sector.Vertex1.Pressure, sector.Vertex1.Temperature,
                sector.Vertex2.Pressure, sector.Vertex2.Temperature,
                sector.Vertex3.Pressure, sector.Vertex3.Temperature,
                sector.C0, sector.C1, sector.C2, sector.C3, sector.C4, sector.C5,
            }.Select(Number).Prepend(sector.Number.ToString(CultureInfo.InvariantCulture))));

        Assert.Equal(20, sensor.Sectors.Count);
    }

    private static string Number(double value) => value.ToString("R", CultureInfo.InvariantCulture);
}

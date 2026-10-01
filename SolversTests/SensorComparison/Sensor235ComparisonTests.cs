using System.Globalization;
using Regression.OutlierDetection;
using Regression.SectorCompensation;
using Xunit.Abstractions;

namespace SolversTests.SensorComparison;

public class Sensor235ComparisonTests
{
    private static readonly int[] NodePressures = { 0, 2, 4, 7, 10 };

    private static readonly CalibrationPoint[] Nodes =
        Sensor235CalibrationPoints.All.Where((_, row) => NodePressures.Contains(row % 11)).ToArray();

    private static readonly CalibrationPoint[] ThreeSeries = Sensor235CalibrationPoints.Series(15, 21, 28);

    private static readonly Dictionary<string, Func<ISensorModel>> AtCalibrationTemperaturesVariants = new()
    {
        ["PolynomialSensor on all 55 points"] = () => new PolynomialSensorCharacterizer().Characterize(Sensor235CalibrationPoints.All),
        ["PolynomialSensor on 25 node points"] = () => new PolynomialSensorCharacterizer().Characterize(Nodes),
        ["SectorSensor 5x5"] = () => new SectorSensorCharacterizer(new PolynomialSensorCharacterizer(), NodePressures).Characterize(Sensor235CalibrationPoints.All),
    };

    private static readonly Dictionary<string, Func<ISensorModel>> BetweenCalibrationTemperaturesVariants = new()
    {
        ["PolynomialSensor"] = () => new PolynomialSensorCharacterizer().Characterize(ThreeSeries),
        ["SectorSensor 3x11"] = () => new SectorSensorCharacterizer(new PolynomialSensorCharacterizer()).Characterize(ThreeSeries),
        ["SectorSensor 3x5"] = () => new SectorSensorCharacterizer(new PolynomialSensorCharacterizer(), NodePressures).Characterize(ThreeSeries),
    };

    private readonly ITestOutputHelper _output;

    public Sensor235ComparisonTests(ITestOutputHelper output) => _output = output;

    [Theory]
    [InlineData("PolynomialSensor on all 55 points", 25, 25)]
    [InlineData("PolynomialSensor on 25 node points", 25, 25)]
    [InlineData("SectorSensor 5x5", 73, 121)]
    public void AtCalibrationTemperatures(string variant, int coefficientCount, int storedValueCount)
    {
        var validation = Sensor235CalibrationPoints.All.Except(Nodes).ToArray();

        Assert.Equal(30, validation.Length);
        Report(variant, AtCalibrationTemperaturesVariants[variant](), validation, coefficientCount, storedValueCount);
    }

    [Theory]
    [InlineData("PolynomialSensor", 25, 25)]
    [InlineData("SectorSensor 3x11", 85, 145)]
    [InlineData("SectorSensor 3x5", 49, 73)]
    public void BetweenCalibrationTemperatures(string variant, int coefficientCount, int storedValueCount)
    {
        var validation = Sensor235CalibrationPoints.Series(18, 24.5);

        Assert.Equal(22, validation.Length);
        Report(variant, BetweenCalibrationTemperaturesVariants[variant](), validation, coefficientCount, storedValueCount);
    }

    private void Report(string variant, ISensorModel model, CalibrationPoint[] validation, int coefficientCount, int storedValueCount)
    {
        var counts = Counts(model);
        var errors = validation
            .Select(point => (point.Temperature, Error: Math.Abs(point.Pressure - model.GetPressure(point.PressureCode, point.TemperatureCode))))
            .ToArray();

        _output.WriteLine($"{variant}: {counts.Coefficients} coefficients, {counts.StoredValues} stored values");
        _output.WriteLine("| Temperature | Points | Max error | Mean error |");
        _output.WriteLine("|---|---|---|---|");
        foreach (var series in errors.GroupBy(error => error.Temperature).OrderBy(series => series.Key))
            _output.WriteLine(Invariant($"| {series.Key} | {series.Count()} | {series.Max(error => error.Error):F4} | {series.Average(error => error.Error):F4} |"));
        _output.WriteLine(Invariant($"| all | {errors.Length} | {errors.Max(error => error.Error):F4} | {errors.Average(error => error.Error):F4} |"));
        if (model is SectorSensor sectorSensor)
            ReportSectorSelection(sectorSensor, validation);

        Assert.Equal(coefficientCount, counts.Coefficients);
        Assert.Equal(storedValueCount, counts.StoredValues);
    }

    private void ReportSectorSelection(SectorSensor sensor, CalibrationPoint[] validation)
    {
        var traces = validation.Select(point => (Point: point, Trace: sensor.Trace(point.PressureCode, point.TemperatureCode))).ToArray();
        var roughOutsideGrid = traces.Count(x => SectorOf(sensor, x.Trace.RoughPressure, x.Trace.RoughTemperature) == 0);
        var roughSectorDiffers = traces.Count(x => SectorOf(sensor, x.Trace.RoughPressure, x.Trace.RoughTemperature) != x.Trace.SectorNumber);
        var referenceSectorDiffers = traces.Count(x => !Contains(sensor.Sectors[x.Trace.SectorNumber - 1], x.Point.Pressure, x.Trace.RoughTemperature));
        var roughPressureError = traces.Max(x => Math.Abs(x.Trace.RoughPressure - x.Point.Pressure));
        var roughTemperatureError = traces.Max(x => Math.Abs(x.Trace.RoughTemperature - x.Point.Temperature));

        _output.WriteLine(Invariant($"Rough stage: max pressure error {roughPressureError:F4}, max temperature error {roughTemperatureError:F4}"));
        _output.WriteLine($"Rough point outside the grid: {roughOutsideGrid} of {traces.Length}");
        _output.WriteLine($"Rough point in a different sector than the final pressure: {roughSectorDiffers} of {traces.Length}");
        _output.WriteLine($"Reference pressure outside the sector that produced the result: {referenceSectorDiffers} of {traces.Length}");
    }

    private static int SectorOf(SectorSensor sensor, double pressure, double temperature)
        => sensor.Sectors.FirstOrDefault(sector => Contains(sector, pressure, temperature))?.Number ?? 0;

    private static bool Contains(Sector sector, double pressure, double temperature)
    {
        var s1 = Side(sector.Vertex1, sector.Vertex2, pressure, temperature);
        var s2 = Side(sector.Vertex2, sector.Vertex3, pressure, temperature);
        var s3 = Side(sector.Vertex3, sector.Vertex1, pressure, temperature);
        return (s1 >= 0 && s2 >= 0 && s3 >= 0) || (s1 <= 0 && s2 <= 0 && s3 <= 0);
    }

    private static double Side(SectorNode from, SectorNode to, double pressure, double temperature)
        => (to.Pressure - from.Pressure) * (temperature - from.Temperature) - (to.Temperature - from.Temperature) * (pressure - from.Pressure);

    private static (int Coefficients, int StoredValues) Counts(ISensorModel model) => model switch
    {
        PolynomialSensor polynomial => (polynomial.CoefficientCount, polynomial.CoefficientCount),
        SectorSensor sector => (sector.CoefficientCount, sector.StoredValueCount),
        _ => throw new NotSupportedException(model.GetType().Name),
    };

    private static string Invariant(FormattableString text) => text.ToString(CultureInfo.InvariantCulture);
}

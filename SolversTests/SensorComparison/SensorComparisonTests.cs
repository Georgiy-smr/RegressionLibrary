using System.Globalization;
using Regression.OutlierDetection;
using Regression.SectorCompensation;
using Xunit.Abstractions;

namespace SolversTests.SensorComparison;

public class SensorComparisonTests
{
    private const double PrototypeTolerance = 0.0002;

    private static readonly Dictionary<string, ISensorCharacterizer> Variants = new()
    {
        ["PolynomialSensor"] = new PolynomialSensorCharacterizer(),
        ["SectorSensor 5x11"] = new SectorSensorCharacterizer(new PolynomialSensorCharacterizer()),
        ["SectorSensor 5x5"] = new SectorSensorCharacterizer(new PolynomialSensorCharacterizer(), new[] { 0, 2, 4, 7, 10 }),
    };

    private readonly ITestOutputHelper _output;

    public SensorComparisonTests(ITestOutputHelper output) => _output = output;

    [Theory]
    [InlineData("PolynomialSensor", 25, 0.0037, 0.0010)]
    [InlineData("SectorSensor 5x11", 145, 0.0046, 0.0011)]
    [InlineData("SectorSensor 5x5", 73, 0.0126, 0.0040)]
    public void MatchesPrototype(string variant, int coefficientCount, double maxError, double meanError)
    {
        var model = Variants[variant].Characterize(Sensor00249979Dataset.Points);
        var validation = Sensor00249979ValidationGrid.Points.Except(Sensor00249979Dataset.Points).ToArray();
        var errors = validation
            .Select(point => (point.Temperature, Error: Math.Abs(point.Pressure - model.GetPressure(point.PressureCode, point.TemperatureCode))))
            .ToArray();

        _output.WriteLine($"{variant}, {CoefficientCount(model)} coefficients");
        _output.WriteLine("| Temperature | Points | Max error | Mean error |");
        _output.WriteLine("|---|---|---|---|");
        foreach (var series in errors.GroupBy(error => error.Temperature).OrderBy(series => series.Key))
            _output.WriteLine(Invariant($"| {series.Key} | {series.Count()} | {series.Max(error => error.Error):F4} | {series.Average(error => error.Error):F4} |"));
        _output.WriteLine(Invariant($"| all | {errors.Length} | {errors.Max(error => error.Error):F6} | {errors.Average(error => error.Error):F6} |"));

        Assert.Equal(224, validation.Length);
        Assert.Equal(coefficientCount, CoefficientCount(model));
        Assert.InRange(Math.Abs(errors.Max(error => error.Error) - maxError), 0, PrototypeTolerance);
        Assert.InRange(Math.Abs(errors.Average(error => error.Error) - meanError), 0, PrototypeTolerance);
    }

    private static int CoefficientCount(ISensorModel model) => model switch
    {
        PolynomialSensor polynomial => polynomial.CoefficientCount,
        SectorSensor sector => sector.CoefficientCount,
        _ => throw new NotSupportedException(model.GetType().Name),
    };

    private static string Invariant(FormattableString text) => text.ToString(CultureInfo.InvariantCulture);
}

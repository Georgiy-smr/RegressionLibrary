using System.Globalization;
using Regression.ErrorAnalysis;
using Regression.SectorCompensation;
using Regression.Two_factor_regression;
using Regression.Two_factor_regression.Implements;
using Regression.Two_factor_regression.Interfaces.Services;
using Xunit.Abstractions;

namespace SolversTests.SensorComparison;

public class RoughStageTests
{
    private static readonly Dictionary<string, IPolynomialFitService> PressureSolvers = new()
    {
        ["LeastSquares 2nd order"] = new PolynomialLeastSquaresSolver(new SecondOrderBasisExponents()),
        ["LeastSquares 3rd order"] = new PolynomialLeastSquaresSolver(new ThirdOrderBasisExponents()),
        ["LeastSquares 4th order"] = new PolynomialLeastSquaresSolver(new FourthOrderBasisExponents()),
        ["Minimax 3rd order"] = new MinimaxPolynomialSolver(new ThirdOrderBasisExponents()),
    };

    private readonly ITestOutputHelper _output;

    public RoughStageTests(ITestOutputHelper output) => _output = output;

    [Theory]
    [InlineData(9, 9)]
    [InlineData(9, 16)]
    [InlineData(9, 25)]
    [InlineData(16, 9)]
    [InlineData(16, 16)]
    [InlineData(16, 25)]
    [InlineData(25, 9)]
    [InlineData(25, 16)]
    [InlineData(25, 25)]
    public void SupportedCoefficientCounts(int pressureCount, int temperatureCount)
    {
        var sensor = new PolynomialSensor(new double[pressureCount], new double[temperatureCount]);

        Assert.Equal(pressureCount + temperatureCount, sensor.CoefficientCount);
    }

    [Theory]
    [InlineData(12, 9)]
    [InlineData(0, 9)]
    [InlineData(16, 12)]
    [InlineData(16, 0)]
    public void UnsupportedCoefficientCounts(int pressureCount, int temperatureCount)
    {
        Assert.Throws<ArgumentException>(() => new PolynomialSensor(new double[pressureCount], new double[temperatureCount]));
    }

    [Theory]
    [InlineData(9)]
    [InlineData(16)]
    [InlineData(25)]
    public void EvaluationEqualsTwoFactorPolynomialValue(int coefficientCount)
    {
        var coefficients = Enumerable.Range(1, coefficientCount).Select(i => 0.5 / i).ToArray();
        var sensor = new PolynomialSensor(coefficients, coefficients);

        var expected = new TwoFactorPolynomialValue(coefficients, new DataTwoFact { X1 = 1.5, X2 = -0.75 }).Value();

        Assert.Equal(expected, sensor.GetPressure(1.5, -0.75));
        Assert.Equal(expected, sensor.GetTemperature(1.5, -0.75));
    }

    [Fact]
    public void GivenRoughSensorGivesTheSameSectorSensor()
    {
        var characterizer = new SectorSensorCharacterizer(new PolynomialSensorCharacterizer());

        var fitted = characterizer.Characterize(Sensor00249979Dataset.Points);
        var given = characterizer.Characterize(Sensor00249979Dataset.Points, fitted.RoughSensor);

        foreach (var point in Sensor00249979ValidationGrid.Points)
            Assert.Equal(
                fitted.GetPressure(point.PressureCode, point.TemperatureCode),
                given.GetPressure(point.PressureCode, point.TemperatureCode));
        Assert.Equal(279, Sensor00249979ValidationGrid.Points.Length);
        Assert.Equal(fitted.Sectors.SelectMany(sector => sector.Values), given.Sectors.SelectMany(sector => sector.Values));
    }

    [Theory]
    [InlineData("LeastSquares 2nd order", 9)]
    [InlineData("LeastSquares 3rd order", 16)]
    [InlineData("LeastSquares 4th order", 25)]
    [InlineData("Minimax 3rd order", 16)]
    public void SectorsOnPickedRoughStage(string pressureSolver, int pressureCount)
    {
        var points = Sensor00249979Dataset.Points;
        var pressureCoefficients = PressureSolvers[pressureSolver]
            .GetValues(points.Select(point => new DataTwoFact { X1 = point.PressureCode, X2 = point.TemperatureCode, Y = point.Pressure }));
        var temperatureCoefficients = new PolynomialLeastSquaresSolver(new SecondOrderBasisExponents())
            .GetValues(points.Select(point => new DataTwoFact { X1 = point.PressureCode, X2 = point.TemperatureCode, Y = point.Temperature }));
        var roughSensor = new PolynomialSensor(pressureCoefficients, temperatureCoefficients);

        var sensor = new SectorSensorCharacterizer(new PolynomialSensorCharacterizer()).Characterize(points, roughSensor);

        var validation = Sensor00249979ValidationGrid.Points.Except(points).ToArray();
        _output.WriteLine(string.Format(
            CultureInfo.InvariantCulture,
            "{0}: rough stage max error {1:F4}, sector sensor max error {2:F4} on {3} validation points",
            pressureSolver, new SensorModelError(roughSensor, validation).GetMax(), new SensorModelError(sensor, validation).GetMax(), validation.Length));

        foreach (var node in points)
            Assert.InRange(Math.Abs(sensor.GetPressure(node.PressureCode, node.TemperatureCode) - node.Pressure), 0, 1e-9);
        Assert.Equal(pressureCount + 9, roughSensor.CoefficientCount);
        Assert.Equal(pressureCount + 9 + 6 * 20, sensor.CoefficientCount);
        Assert.Equal(pressureCount + 9 + 12 * 20, sensor.StoredValueCount);
    }
}

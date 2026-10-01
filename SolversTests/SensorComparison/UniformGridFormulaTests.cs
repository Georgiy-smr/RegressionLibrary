using Regression.SectorCompensation;

namespace SolversTests.SensorComparison;

public class UniformGridFormulaTests
{
    private const double HP = 60;
    private const double HT = 3;

    private static readonly double[] Pressures = { 10, 70, 130, 190, 250 };
    private static readonly double[] Temperatures = { 15, 18, 21, 24, 27 };

    [Fact]
    public void LowerTriangleOfFirstQuadrant()
    {
        var sensor = new SectorSensorCharacterizer(new PolynomialSensorCharacterizer())
            .Characterize(SyntheticGrid.Build(Pressures, Temperatures, PressureCode));

        var sector = sensor.Sectors[0];

        AssertClose(F(0, 0), sector.Coefficients[0]);
        AssertClose((4 * F(1, 0) - F(2, 0) - 3 * F(0, 0)) / (2 * HP), sector.Coefficients[1]);
        AssertClose((4 * F(0, 1) - F(0, 2) - 3 * F(0, 0)) / (2 * HT), sector.Coefficients[2]);
        AssertClose((F(2, 0) - 2 * F(1, 0) + F(0, 0)) / (2 * HP * HP), sector.Coefficients[3]);
        AssertClose((F(0, 2) - 2 * F(0, 1) + F(0, 0)) / (2 * HT * HT), sector.Coefficients[4]);
        AssertClose((F(1, 1) - F(1, 0) - F(0, 1) + F(0, 0)) / (HP * HT), sector.Coefficients[5]);
    }

    private static void AssertClose(double expected, double actual)
        => Assert.InRange(Math.Abs(actual - expected), 0, 1e-6 * Math.Abs(expected));

    private static double F(int i, int j) => PressureCode(Pressures[i], Temperatures[j]);

    private static double PressureCode(double pressure, double temperature)
        => 250000 + 17900 * pressure - 5800 * temperature - 0.35 * pressure * pressure + 1.2 * temperature * temperature - 37 * pressure * temperature
           + 0.0004 * pressure * pressure * pressure + 0.02 * pressure * temperature * temperature;
}

using Regression.ErrorAnalysis;
using Regression.Two_factor_regression;

namespace Regression.SectorCompensation;

public sealed class PolynomialSensor : ISensorModel
{
    private static readonly int[] SupportedCoefficientCounts = { 9, 16, 25 };

    private readonly double[] _pressureCoefficients;
    private readonly double[] _temperatureCoefficients;

    public PolynomialSensor(IEnumerable<double> pressureCoefficients, IEnumerable<double> temperatureCoefficients)
    {
        if (pressureCoefficients is null) throw new ArgumentNullException(nameof(pressureCoefficients));
        if (temperatureCoefficients is null) throw new ArgumentNullException(nameof(temperatureCoefficients));

        _pressureCoefficients = pressureCoefficients.ToArray();
        _temperatureCoefficients = temperatureCoefficients.ToArray();

        if (!SupportedCoefficientCounts.Contains(_pressureCoefficients.Length))
            throw new ArgumentException($"The pressure polynomial must have 9, 16 or 25 coefficients, but got {_pressureCoefficients.Length}.", nameof(pressureCoefficients));
        if (!SupportedCoefficientCounts.Contains(_temperatureCoefficients.Length))
            throw new ArgumentException($"The temperature polynomial must have 9, 16 or 25 coefficients, but got {_temperatureCoefficients.Length}.", nameof(temperatureCoefficients));
    }

    public IReadOnlyList<double> PressureCoefficients => _pressureCoefficients;

    public IReadOnlyList<double> TemperatureCoefficients => _temperatureCoefficients;

    public int CoefficientCount => _pressureCoefficients.Length + _temperatureCoefficients.Length;

    public double GetPressure(double pressureCode, double temperatureCode)
        => new TwoFactorPolynomialValue(_pressureCoefficients, new DataTwoFact { X1 = pressureCode, X2 = temperatureCode }).Value();

    public double GetTemperature(double pressureCode, double temperatureCode)
        => new TwoFactorPolynomialValue(_temperatureCoefficients, new DataTwoFact { X1 = pressureCode, X2 = temperatureCode }).Value();
}

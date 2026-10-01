using Regression.ErrorAnalysis;
using Regression.Two_factor_regression;

namespace Regression.SectorCompensation;

public sealed class PolynomialSensor : ISensorModel
{
    private const int PressureCoefficientCount = 16;
    private const int TemperatureCoefficientCount = 9;

    private readonly double[] _pressureCoefficients;
    private readonly double[] _temperatureCoefficients;

    public PolynomialSensor(IEnumerable<double> pressureCoefficients, IEnumerable<double> temperatureCoefficients)
    {
        if (pressureCoefficients is null) throw new ArgumentNullException(nameof(pressureCoefficients));
        if (temperatureCoefficients is null) throw new ArgumentNullException(nameof(temperatureCoefficients));

        _pressureCoefficients = pressureCoefficients.ToArray();
        _temperatureCoefficients = temperatureCoefficients.ToArray();

        if (_pressureCoefficients.Length != PressureCoefficientCount)
            throw new ArgumentException($"The pressure polynomial must have {PressureCoefficientCount} coefficients, but got {_pressureCoefficients.Length}.", nameof(pressureCoefficients));
        if (_temperatureCoefficients.Length != TemperatureCoefficientCount)
            throw new ArgumentException($"The temperature polynomial must have {TemperatureCoefficientCount} coefficients, but got {_temperatureCoefficients.Length}.", nameof(temperatureCoefficients));
    }

    public IReadOnlyList<double> PressureCoefficients => _pressureCoefficients;

    public IReadOnlyList<double> TemperatureCoefficients => _temperatureCoefficients;

    public int CoefficientCount => _pressureCoefficients.Length + _temperatureCoefficients.Length;

    public double GetPressure(double pressureCode, double temperatureCode)
        => new TwoFactorPolynomialValue(_pressureCoefficients, new DataTwoFact { X1 = pressureCode, X2 = temperatureCode }).Value();

    public double GetTemperature(double pressureCode, double temperatureCode)
        => new TwoFactorPolynomialValue(_temperatureCoefficients, new DataTwoFact { X1 = pressureCode, X2 = temperatureCode }).Value();
}

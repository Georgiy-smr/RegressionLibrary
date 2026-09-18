namespace Regression.ErrorAnalysis;

/// <summary>
/// Power polynomial evaluated via Horner's formula, matching Approximator.CorValue:
/// coefficients[0] is the highest-degree coefficient.
/// </summary>
public sealed class HornerPolynomialValue : IPolynomialValue
{
    private readonly double[] _coefficients;
    private readonly double _x;

    public HornerPolynomialValue(double[] coefficients, double x)
    {
        _coefficients = coefficients;
        _x = x;
    }

    public double Value()
    {
        double result = 0;
        for (int i = 0; i < _coefficients.Length; i++)
            result += _coefficients[i] * Math.Pow(_x, _coefficients.Length - (i + 1));

        return result;
    }
}

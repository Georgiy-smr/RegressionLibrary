using Regression.Two_factor_regression;

namespace Regression.ErrorAnalysis;

/// <summary>
/// Two-factor polynomial value. Selects the concrete coefficient-count implementation
/// (16 or 9 — see AnalyzeFewRegression.CalcResDelta) and forwards to it.
/// </summary>
public sealed class TwoFactorPolynomialValue : IPolynomialValue
{
    private readonly IPolynomialValue _origin;

    public TwoFactorPolynomialValue(double[] coefficients, DataTwoFact data)
        : this(coefficients.Length switch
        {
            16 => (IPolynomialValue)new TwoFactorPolynomialValue16(coefficients, data),
            9 => new TwoFactorPolynomialValue9(coefficients, data),
            _ => throw new NotSupportedException(
                $"Unsupported coefficient array length: {coefficients.Length}. Only 16 and 9 are supported.")
        })
    {
    }

    private TwoFactorPolynomialValue(IPolynomialValue origin)
    {
        _origin = origin;
    }

    public double Value() => _origin.Value();
}

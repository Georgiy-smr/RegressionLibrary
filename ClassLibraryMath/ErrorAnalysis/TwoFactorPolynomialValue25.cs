using Regression.Two_factor_regression;

namespace Regression.ErrorAnalysis;

/// <summary>
/// Two-factor fourth-order polynomial for the 25-coefficient form, in FourthOrderBasisExponents
/// order: a0..a15 are the same terms as TwoFactorPolynomialValue16, a16..a24 the degree-4 terms.
/// </summary>
public sealed class TwoFactorPolynomialValue25 : IPolynomialValue
{
    private readonly double[] _coefficients;
    private readonly DataTwoFact _data;

    public TwoFactorPolynomialValue25(double[] coefficients, DataTwoFact data)
    {
        _coefficients = coefficients;
        _data = data;
    }

    public double Value()
    {
        double res = 0;

        for (int i = 0; i < _coefficients.Length; i++)
        {
            switch (i)
            {
                case 0: res += _coefficients[i]; break;
                case 1: res += _coefficients[i] * _data.X2; break;
                case 2: res += _coefficients[i] * Math.Pow(_data.X2, 2); break;
                case 3: res += _coefficients[i] * _data.X1; break;
                case 4: res += _coefficients[i] * Math.Pow(_data.X1, 2); break;
                case 5: res += _coefficients[i] * _data.X1 * _data.X2; break;
                case 6: res += _coefficients[i] * Math.Pow(_data.X1, 2) * _data.X2; break;
                case 7: res += _coefficients[i] * _data.X1 * Math.Pow(_data.X2, 2); break;
                case 8: res += _coefficients[i] * Math.Pow(_data.X1, 2) * Math.Pow(_data.X2, 2); break;
                case 9: res += _coefficients[i] * Math.Pow(_data.X1, 3); break;
                case 10: res += _coefficients[i] * Math.Pow(_data.X1, 3) * _data.X2; break;
                case 11: res += _coefficients[i] * Math.Pow(_data.X1, 3) * Math.Pow(_data.X2, 2); break;
                case 12: res += _coefficients[i] * Math.Pow(_data.X2, 3); break;
                case 13: res += _coefficients[i] * _data.X1 * Math.Pow(_data.X2, 3); break;
                case 14: res += _coefficients[i] * Math.Pow(_data.X1, 2) * Math.Pow(_data.X2, 3); break;
                case 15: res += _coefficients[i] * Math.Pow(_data.X1, 3) * Math.Pow(_data.X2, 3); break;
                case 16: res += _coefficients[i] * Math.Pow(_data.X1, 4); break;
                case 17: res += _coefficients[i] * Math.Pow(_data.X1, 4) * _data.X2; break;
                case 18: res += _coefficients[i] * Math.Pow(_data.X1, 4) * Math.Pow(_data.X2, 2); break;
                case 19: res += _coefficients[i] * Math.Pow(_data.X1, 4) * Math.Pow(_data.X2, 3); break;
                case 20: res += _coefficients[i] * Math.Pow(_data.X2, 4); break;
                case 21: res += _coefficients[i] * _data.X1 * Math.Pow(_data.X2, 4); break;
                case 22: res += _coefficients[i] * Math.Pow(_data.X1, 2) * Math.Pow(_data.X2, 4); break;
                case 23: res += _coefficients[i] * Math.Pow(_data.X1, 3) * Math.Pow(_data.X2, 4); break;
                case 24: res += _coefficients[i] * Math.Pow(_data.X1, 4) * Math.Pow(_data.X2, 4); break;
            }
        }

        return res;
    }
}

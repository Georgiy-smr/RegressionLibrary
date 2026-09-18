using Regression.Two_factor_regression;

namespace Regression.ErrorAnalysis;

/// <summary>
/// Two-factor third-order polynomial for the 16-coefficient form
/// (see ExpressionCreator.CreateThirdOrderPolynomialExpression).
/// </summary>
public sealed class TwoFactorPolynomialValue16 : IPolynomialValue
{
    private readonly double[] _coefficients;
    private readonly DataTwoFact _data;

    public TwoFactorPolynomialValue16(double[] coefficients, DataTwoFact data)
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
                case 0:
                    res += _coefficients[i];
                    break;
                case 1:
                    res += _coefficients[i] * _data.X2;
                    break;
                case 2:
                    res += _coefficients[i] * _data.X2 * _data.X2;
                    break;
                case 3:
                    res += _coefficients[i] * _data.X1;
                    break;
                case 4:
                    res += _coefficients[i] * _data.X1 * _data.X1;
                    break;
                case 5:
                    res += _coefficients[i] * _data.X1 * _data.X2;
                    break;
                case 6:
                    res += _coefficients[i] * _data.X2 * _data.X1 * _data.X1;
                    break;
                case 7:
                    res += _coefficients[i] * _data.X2 * _data.X2 * _data.X1;
                    break;
                case 8:
                    res += _coefficients[i] * _data.X1 * _data.X1 * _data.X2 * _data.X2;
                    break;
                case 9:
                    res += _coefficients[i] * _data.X1 * _data.X1 * _data.X1;
                    break;
                case 10:
                    res += _coefficients[i] * _data.X2 * _data.X1 * _data.X1 * _data.X1;
                    break;
                case 11:
                    res += _coefficients[i] * _data.X2 * _data.X2 * _data.X1 * _data.X1 * _data.X1;
                    break;
                case 12:
                    res += _coefficients[i] * _data.X2 * _data.X2 * _data.X2;
                    break;
                case 13:
                    res += _coefficients[i] * _data.X2 * _data.X2 * _data.X2 * _data.X1;
                    break;
                case 14:
                    res += _coefficients[i] * _data.X2 * _data.X2 * _data.X2 * _data.X1 * _data.X1;
                    break;
                case 15:
                    res += _coefficients[i] * _data.X2 * _data.X2 * _data.X2 * _data.X1 * _data.X1 * _data.X1;
                    break;
            }
        }

        return res;
    }
}

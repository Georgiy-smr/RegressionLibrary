using Regression.Two_factor_regression;

namespace Regression.ErrorAnalysis;

/// <summary>
/// Compares the accuracy of different linear-system solvers by measuring the error
/// between approximated/regressed values and the original data, either for the
/// two-factor regression polynomial or for the single-variable power approximation.
/// </summary>
public class ApproximationCalculationError
{
    private enum Mode
    {
        TwoFactorRegression,
        SingleVariableApproximation
    }

    private readonly Mode _mode;
    private readonly double[] _coefficients;
    private readonly DataTwoFact[]? _data;
    private readonly double[]? _x;
    private readonly double[]? _y;

    /// <summary>
    /// Two-factor regression mode.
    /// </summary>
    public ApproximationCalculationError(IEnumerable<double> coefficients, IEnumerable<DataTwoFact> data)
    {
        _coefficients = coefficients.ToArray();
        _data = data.ToArray();
        _mode = Mode.TwoFactorRegression;
    }

    /// <summary>
    /// Single-variable power approximation mode. <paramref name="x"/> and <paramref name="y"/>
    /// must be the same arrays passed into Approximator.CalcCoeffs.
    /// </summary>
    public ApproximationCalculationError(IEnumerable<double> coefficients, double[] x, double[] y)
    {
        if (x.Length != y.Length)
            throw new ArgumentException("x and y must have the same length.");

        _coefficients = coefficients.ToArray();
        _x = x;
        _y = y;
        _mode = Mode.SingleVariableApproximation;
    }

    /// <summary>
    /// Maximum absolute error across the whole stored dataset.
    /// </summary>
    public double GetMax()
    {
        if (_mode == Mode.TwoFactorRegression)
            return _data!.Select(item => Math.Abs(item.Y - CalcTwoFactorValue(item, _coefficients))).Max();

        return _x!.Select((xi, i) => Math.Abs(_y![i] - CalcHornerValue(xi))).Max();
    }

    /// <summary>
    /// Two-factor regression mode only.
    /// </summary>
    public double GetCurrent(DataTwoFact data, double expectedValue)
    {
        if (_mode != Mode.TwoFactorRegression)
            throw new InvalidOperationException($"{nameof(GetCurrent)}(DataTwoFact, double) is only available in two-factor regression mode.");

        return Math.Abs(expectedValue - CalcTwoFactorValue(data, _coefficients));
    }

    /// <summary>
    /// Single-variable approximation mode only.
    /// </summary>
    public double GetCurrent(double x, double expectedValue)
    {
        if (_mode != Mode.SingleVariableApproximation)
            throw new InvalidOperationException($"{nameof(GetCurrent)}(double, double) is only available in single-variable approximation mode.");

        return Math.Abs(expectedValue - CalcHornerValue(x));
    }

    private double CalcHornerValue(double x)
    {
        double result = 0;
        for (int i = 0; i < _coefficients.Length; i++)
            result += _coefficients[i] * Math.Pow(x, _coefficients.Length - (i + 1));

        return result;
    }

    private static double CalcTwoFactorValue(DataTwoFact data, double[] coeffs)
    {
        double res = 0;

        if (coeffs.Length == 16)
        {
            for (int i = 0; i < coeffs.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        res += coeffs[i];
                        break;
                    case 1:
                        res += coeffs[i] * data.X2;
                        break;
                    case 2:
                        res += coeffs[i] * data.X2 * data.X2;
                        break;
                    case 3:
                        res += coeffs[i] * data.X1;
                        break;
                    case 4:
                        res += coeffs[i] * data.X1 * data.X1;
                        break;
                    case 5:
                        res += coeffs[i] * data.X1 * data.X2;
                        break;
                    case 6:
                        res += coeffs[i] * data.X2 * data.X1 * data.X1;
                        break;
                    case 7:
                        res += coeffs[i] * data.X2 * data.X2 * data.X1;
                        break;
                    case 8:
                        res += coeffs[i] * data.X1 * data.X1 * data.X2 * data.X2;
                        break;
                    case 9:
                        res += coeffs[i] * data.X1 * data.X1 * data.X1;
                        break;
                    case 10:
                        res += coeffs[i] * data.X2 * data.X1 * data.X1 * data.X1;
                        break;
                    case 11:
                        res += coeffs[i] * data.X2 * data.X2 * data.X1 * data.X1 * data.X1;
                        break;
                    case 12:
                        res += coeffs[i] * data.X2 * data.X2 * data.X2;
                        break;
                    case 13:
                        res += coeffs[i] * data.X2 * data.X2 * data.X2 * data.X1;
                        break;
                    case 14:
                        res += coeffs[i] * data.X2 * data.X2 * data.X2 * data.X1 * data.X1;
                        break;
                    case 15:
                        res += coeffs[i] * data.X2 * data.X2 * data.X2 * data.X1 * data.X1 * data.X1;
                        break;
                }
            }
        }
        else if (coeffs.Length == 9)
        {
            for (int i = 0; i < coeffs.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        res += coeffs[i];
                        break;
                    case 1:
                        res += coeffs[i] * data.X2;
                        break;
                    case 2:
                        res += coeffs[i] * data.X2 * data.X2;
                        break;
                    case 3:
                        res += coeffs[i] * data.X1;
                        break;
                    case 4:
                        res += coeffs[i] * data.X1 * data.X1;
                        break;
                    case 5:
                        res += coeffs[i] * data.X1 * data.X2;
                        break;
                    case 6:
                        res += coeffs[i] * data.X2 * data.X1 * data.X1;
                        break;
                    case 7:
                        res += coeffs[i] * data.X2 * data.X2 * data.X1;
                        break;
                    case 8:
                        res += coeffs[i] * data.X1 * data.X1 * data.X2 * data.X2;
                        break;
                }
            }
        }
        else
        {
            throw new NotSupportedException($"Unsupported coefficient array length: {coeffs.Length}. Only 16 and 9 are supported.");
        }

        return res;
    }
}

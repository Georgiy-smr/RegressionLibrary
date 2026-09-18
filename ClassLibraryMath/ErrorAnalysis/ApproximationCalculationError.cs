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
            return _data!.Select(item => Math.Abs(item.Y - new TwoFactorPolynomialValue(_coefficients, item).Value())).Max();

        return _x!.Select((xi, i) => Math.Abs(_y![i] - new HornerPolynomialValue(_coefficients, xi).Value())).Max();
    }

    /// <summary>
    /// Two-factor regression mode only.
    /// </summary>
    public double GetCurrent(DataTwoFact data, double expectedValue)
    {
        if (_mode != Mode.TwoFactorRegression)
            throw new InvalidOperationException($"{nameof(GetCurrent)}(DataTwoFact, double) is only available in two-factor regression mode.");

        return Math.Abs(expectedValue - new TwoFactorPolynomialValue(_coefficients, data).Value());
    }

    /// <summary>
    /// Single-variable approximation mode only.
    /// </summary>
    public double GetCurrent(double x, double expectedValue)
    {
        if (_mode != Mode.SingleVariableApproximation)
            throw new InvalidOperationException($"{nameof(GetCurrent)}(double, double) is only available in single-variable approximation mode.");

        return Math.Abs(expectedValue - new HornerPolynomialValue(_coefficients, x).Value());
    }
}

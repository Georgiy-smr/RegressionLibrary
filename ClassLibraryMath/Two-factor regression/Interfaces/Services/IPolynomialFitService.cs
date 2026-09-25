namespace Regression.Two_factor_regression.Interfaces.Services;

/// <summary>
/// Common contract for fitting a two-factor polynomial over an <see cref="IBasisExponents"/>,
/// whatever the fitting criterion (least squares, minimax, ...).
/// </summary>
public interface IPolynomialFitService
{
    /// <summary>
    /// Fits the polynomial to <paramref name="data"/> and returns the coefficients of the
    /// original monomials X1^i * X2^j, in the fixed order of the solver's IBasisExponents.
    /// </summary>
    IEnumerable<double> GetValues(IEnumerable<DataTwoFact> data);
}

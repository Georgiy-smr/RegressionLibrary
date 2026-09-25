namespace Regression.Two_factor_regression.Interfaces.Services;

/// <summary>
/// A two-factor polynomial fit that minimizes the sum of squared residuals.
/// <see cref="IPolynomialFitService.GetValues"/> is inherited so existing consumers keep compiling.
/// </summary>
public interface ILeastSquaresRegressionService : IPolynomialFitService
{
}

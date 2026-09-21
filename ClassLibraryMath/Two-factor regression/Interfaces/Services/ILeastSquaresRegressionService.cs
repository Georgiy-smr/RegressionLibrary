namespace Regression.Two_factor_regression.Interfaces.Services;

/// <summary>
/// Fits a two-factor regression directly from data (as opposed to
/// IRegressionAnalysisService.GetValues(IPolynomialExpression), which requires the caller to
/// have already built a symbolic expression via ExpressionCreator). Deliberately not tied to a
/// specific polynomial order — the order is a property of the concrete implementation, the way
/// IRegressionAnalysisService callers pick the order via which ExpressionCreator method they
/// call.
/// </summary>
public interface ILeastSquaresRegressionService
{
    IEnumerable<double> GetValues(IEnumerable<DataTwoFact> data);
}

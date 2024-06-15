namespace Regression.Two_factor_regression.Interfaces.Services;

public interface IRegressionAnalysisService
{
    IEnumerable<double> GetValues(IPolynomialExpression expression);
}
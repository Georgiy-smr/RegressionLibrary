namespace Regression.Two_factor_regression.Interfaces.Services;

public interface ILeastSquaresRegressionService
{
    IEnumerable<double> GetValues(IEnumerable<DataTwoFact> data);
}

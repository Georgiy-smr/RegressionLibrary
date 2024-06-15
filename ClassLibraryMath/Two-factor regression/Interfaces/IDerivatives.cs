namespace Regression.Two_factor_regression.Interfaces;
/// <summary>
/// Вычисление производной 
/// </summary>
public interface IDerivatives
{
    IEnumerable<string[]> Calculate(IPolynomialExpression expression);
}
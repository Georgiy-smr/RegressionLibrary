namespace Regression.Two_factor_regression.Interfaces;
using MathNet.Symbolics;
/// <summary>
/// Функция регресса и её переменные
/// </summary>
public interface IPolynomialExpression
{
    SymbolicExpression Function { get; }
    IEnumerable<SymbolicExpression> Variables { get; }
}
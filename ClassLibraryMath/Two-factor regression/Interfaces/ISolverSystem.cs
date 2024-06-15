namespace Regression.Two_factor_regression.Interfaces;
/// <summary>
/// Вычислитель корней из матрицы входа и выхода
/// </summary>
public interface ISolverSystem
{
    IEnumerable<double> GetRoots(double[,] X, double[,] Y);
}
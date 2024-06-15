namespace Regression.Two_factor_regression.Interfaces;
/// <summary>
/// Перевод массива строк в матрицу регресса
/// </summary>
public interface IRowsParser
{
    IEnumerable<double> GetRowInput(string[] row,
        int countMembers,
        string targetName = "a");
    IEnumerable<double> GetRowOutput(string[] column,
        int countMembers,
        string targetName = "a");
}
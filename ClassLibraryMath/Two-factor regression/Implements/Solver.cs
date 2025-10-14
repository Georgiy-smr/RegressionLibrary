using MathNet.Numerics.LinearAlgebra;
using Regression.MathService;
using Regression.Two_factor_regression.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regression.Two_factor_regression.Implements
{
    public class Solver : ISolverSystem
    {
        private readonly Gaus gaus = new Gaus();
        public IEnumerable<double> GetRoots(double[,] X, double[,] Y) => gaus.Roots(X, Y);
    }

    public class SolverMathNet : ISolverSystem
    {
        public IEnumerable<double> GetRoots(double[,] X, double[,] Y)
        {
            // Создаем матрицу A из массива X
            Matrix<double>? A = Matrix<double>.Build.DenseOfArray(X);

            // Создаем вектор b из массива Y
            // Предполагается, что Y — двумерный массив размерности m x 1
            int rows = Y.GetLength(0);
            int cols = Y.GetLength(1);
            if (cols != 1)
                throw new ArgumentException("Y должен быть вектором-столбцом (m x 1)");

            Vector<double>? b = Vector<double>.Build.Dense(rows, i => Y[i, 0]);

            // Решаем систему A * x = b
            Vector<double> x = A.Solve(b);

            return x;
        }
    }


}

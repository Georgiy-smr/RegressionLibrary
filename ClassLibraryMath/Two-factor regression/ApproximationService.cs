using Regression.Two_factor_regression.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Regression.Two_factor_regression.Interfaces;

namespace Regression.Two_factor_regression
{
    public class ApproximationService : IRegressionAnalysisService
    {
        public ApproximationService(
            ISolverSystem solverSystem,
            IRowsParser matrixParser,
            IDerivatives differentiator)
        {
            _solverSystem = solverSystem;
            _rowsParser = matrixParser;
            _differentiator = differentiator;
        }
        private readonly ISolverSystem _solverSystem;
        private readonly IRowsParser _rowsParser;
        private readonly IDerivatives _differentiator;
        public IEnumerable<double> GetValues(IPolynomialExpression expression)
        {
            var countExpr = expression.Variables.Count();
            var polysList = _differentiator.Calculate(expression).ToList();
            return _solverSystem.GetRoots(
                BuildMatrix(countExpr, polysList, isInput: true),
                BuildMatrix(countExpr, polysList, isInput: false));
        }
        private double[,] BuildMatrix(int countExpr, List<string[]> polysList, bool isInput)
            => isInput ? BuildInputMatrix(countExpr, polysList) : BuildOutputMatrix(countExpr, polysList);

        private double[,] BuildInputMatrix(int countExpr, List<string[]> polysList)
        {
            double[,] X = new double[countExpr, countExpr];
            for (int i = 0; i < X.GetUpperBound(0) + 1; i++)
            {
                var currentRow = _rowsParser.GetRowInput(polysList[i], countExpr).ToArray();
                for (int j = 0; j < X.GetUpperBound(1) + 1; j++)
                    X[i, j] += currentRow[j];
            }
            return X;
        }
        private double[,] BuildOutputMatrix(int countExpr, List<string[]> polysList)
        {
            double[,] Y = new double[countExpr, 1];
            for (int i = 0; i < Y.GetUpperBound(0) + 1; i++)
            {
                var currentRow = _rowsParser.GetRowOutput(polysList[i], countExpr).ToArray();
                for (int j = 0; j < Y.GetUpperBound(1) + 1; j++)
                    Y[i, 0] += currentRow[j];
            }
            return Y;
        }
    }
}

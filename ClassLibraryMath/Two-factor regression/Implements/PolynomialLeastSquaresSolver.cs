using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearRegression;
using MathNet.Symbolics;
using Regression.Two_factor_regression.Interfaces;
using Regression.Two_factor_regression.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Regression.Two_factor_regression.Implements
{
    public class PolynomialLeastSquaresSolver : ILeastSquaresRegressionService
    {
        private readonly (int PowerOfX1, int PowerOfX2)[] _basisExponents;

        public PolynomialLeastSquaresSolver(IBasisExponents basisExponents)
        {
            _basisExponents = basisExponents.ToArray();
        }

        public IEnumerable<double> GetValues(IEnumerable<DataTwoFact> data)
        {
            var points = data.ToArray();
            if (points.Length < _basisExponents.Length)
                throw new ArgumentOutOfRangeException(nameof(data));

            var (mean1, scale1) = MeanAndScale(points.Select(p => p.X1));
            var (mean2, scale2) = MeanAndScale(points.Select(p => p.X2));

            var design = Matrix<double>.Build.Dense(points.Length, _basisExponents.Length, (row, col) =>
            {
                var x1c = (points[row].X1 - mean1) / scale1;
                var x2c = (points[row].X2 - mean2) / scale2;
                return BasisTerm(_basisExponents[col], x1c, x2c);
            });
            var y = Vector<double>.Build.Dense(points.Length, row => points[row].Y);

            var centeredCoefficients = MultipleRegression.QR(design, y);

            return ConvertToOriginalBasis(centeredCoefficients, mean1, scale1, mean2, scale2);
        }

        private static (double Mean, double Scale) MeanAndScale(IEnumerable<double> values)
        {
            var array = values.ToArray();
            var mean = array.Average();
            var variance = array.Select(v => (v - mean) * (v - mean)).Average();
            var scale = Math.Sqrt(variance);
            return (mean, scale == 0 ? 1 : scale);
        }

        private static double BasisTerm((int PowerOfX1, int PowerOfX2) exponent, double x1, double x2)
            => Math.Pow(x1, exponent.PowerOfX1) * Math.Pow(x2, exponent.PowerOfX2);

        private static SymbolicExpression BasisTerm(
            (int PowerOfX1, int PowerOfX2) exponent, SymbolicExpression x1, SymbolicExpression x2)
        {
            SymbolicExpression term = 1;
            for (int i = 0; i < exponent.PowerOfX1; i++) term *= x1;
            for (int i = 0; i < exponent.PowerOfX2; i++) term *= x2;
            return term;
        }

        private double[] ConvertToOriginalBasis(
            Vector<double> centeredCoefficients, double mean1, double scale1, double mean2, double scale2)
        {
            SymbolicExpression x1 = SymbolicExpression.Variable("X1");
            SymbolicExpression x2 = SymbolicExpression.Variable("X2");
            SymbolicExpression x1c = (x1 - mean1) / scale1;
            SymbolicExpression x2c = (x2 - mean2) / scale2;

            SymbolicExpression fitted = _basisExponents
                .Select((exponent, k) => (SymbolicExpression)centeredCoefficients[k] * BasisTerm(exponent, x1c, x2c))
                .Aggregate((SymbolicExpression)0, (sum, term) => sum + term);

            return _basisExponents
                .Select(exponent => MonomialCoefficient(fitted, x1, x2, exponent.PowerOfX1, exponent.PowerOfX2))
                .ToArray();
        }

        private static double MonomialCoefficient(
            SymbolicExpression expression, SymbolicExpression x1, SymbolicExpression x2, int powerOfX1, int powerOfX2)
        {
            var derivative = expression;
            for (int i = 0; i < powerOfX1; i++) derivative = derivative.Differentiate(x1);
            for (int i = 0; i < powerOfX2; i++) derivative = derivative.Differentiate(x2);

            var atOrigin = new Dictionary<string, FloatingPoint>
            {
                ["X1"] = (FloatingPoint)0.0,
                ["X2"] = (FloatingPoint)0.0,
            };
            return derivative.Evaluate(atOrigin).RealValue / (Factorial(powerOfX1) * Factorial(powerOfX2));
        }

        private static double Factorial(int n) => n <= 1 ? 1 : n * Factorial(n - 1);
    }
}

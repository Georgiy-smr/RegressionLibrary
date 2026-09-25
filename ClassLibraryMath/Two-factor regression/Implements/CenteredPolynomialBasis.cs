using MathNet.Numerics.LinearAlgebra;
using MathNet.Symbolics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Regression.Two_factor_regression.Implements
{
    /// <summary>
    /// A two-factor polynomial basis over centered/scaled variables
    /// x1c = (X1 - mean1) / scale1 and x2c = (X2 - mean2) / scale2, where mean/scale come from
    /// the fitted points. Fitting in this basis keeps the design matrix well-conditioned
    /// (see issue #5); <see cref="ConvertToOriginalBasis"/> maps the fitted coefficients back to
    /// the original monomial basis X1^i * X2^j in the order of the given basis exponents.
    /// Shared by every solver that fits a two-factor polynomial over an IBasisExponents.
    /// </summary>
    internal sealed class CenteredPolynomialBasis
    {
        private readonly (int PowerOfX1, int PowerOfX2)[] _basisExponents;
        private readonly double _mean1;
        private readonly double _scale1;
        private readonly double _mean2;
        private readonly double _scale2;

        public CenteredPolynomialBasis((int PowerOfX1, int PowerOfX2)[] basisExponents, DataTwoFact[] points)
        {
            _basisExponents = basisExponents;
            (_mean1, _scale1) = MeanAndScale(points.Select(p => p.X1));
            (_mean2, _scale2) = MeanAndScale(points.Select(p => p.X2));
        }

        public Matrix<double> DesignMatrix(DataTwoFact[] points)
            => Matrix<double>.Build.Dense(points.Length, _basisExponents.Length, (row, col) =>
            {
                var x1c = (points[row].X1 - _mean1) / _scale1;
                var x2c = (points[row].X2 - _mean2) / _scale2;
                return BasisTerm(_basisExponents[col], x1c, x2c);
            });

        /// <summary>
        /// Symbolic expand/differentiate via MathNet.Symbolics — by far the slowest step of any
        /// fit. Call it once per fit, on the final centered coefficients only.
        /// </summary>
        public double[] ConvertToOriginalBasis(Vector<double> centeredCoefficients)
        {
            SymbolicExpression x1 = SymbolicExpression.Variable("X1");
            SymbolicExpression x2 = SymbolicExpression.Variable("X2");
            SymbolicExpression x1c = (x1 - _mean1) / _scale1;
            SymbolicExpression x2c = (x2 - _mean2) / _scale2;

            SymbolicExpression fitted = _basisExponents
                .Select((exponent, k) => (SymbolicExpression)centeredCoefficients[k] * BasisTerm(exponent, x1c, x2c))
                .Aggregate((SymbolicExpression)0, (sum, term) => sum + term);

            return _basisExponents
                .Select(exponent => MonomialCoefficient(fitted, x1, x2, exponent.PowerOfX1, exponent.PowerOfX2))
                .ToArray();
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

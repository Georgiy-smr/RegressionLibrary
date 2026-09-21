using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearRegression;
using MathNet.Symbolics;
using Regression.Two_factor_regression.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Regression.Two_factor_regression.Implements
{
    /// <summary>
    /// Fits the same 16-term third-order two-factor polynomial as
    /// ExpressionCreator.CreateThirdOrderPolynomialExpression (a0..a15 basis, see
    /// TwoFactorPolynomialValue16), but through direct least squares on a design matrix
    /// instead of symbolic differentiation and normal equations.
    ///
    /// Issue #5 / PR #6: ApproximationService.BuildMatrix forms the normal-equations matrix
    /// XᵀX from raw, uncentered X1/X2 (e.g. X2 ~ -645..-663) raised up to the 3rd power, which
    /// is catastrophically ill-conditioned (~1E+32, see ConditionNumberHypothesisTests) — a
    /// direct double-precision solve on that matrix is expected to lose all significant
    /// digits. This service avoids that by (1) centering and scaling X1/X2 before raising them
    /// to high powers, which keeps every design-matrix column near unit magnitude, and (2)
    /// solving the (still overdetermined) design matrix directly via QR rather than forming
    /// XᵀX at all. The fitted centered-basis coefficients are then converted back into the
    /// original a0..a15 basis symbolically (substitute the centering/scaling formulas and
    /// collect coefficients of each raw monomial), so GetValues() returns coefficients that
    /// TwoFactorPolynomialValue16 / ApproximationCalculationError can consume unchanged.
    /// </summary>
    public class LeastSquaresApproximationServiceThirdOrder : ILeastSquaresRegressionService
    {
        // (power of X1, power of X2) for each a0..a15 basis term, in the exact order defined by
        // ExpressionCreator.CreateThirdOrderPolynomialExpression / TwoFactorPolynomialValue16.
        private static readonly (int PowerOfX1, int PowerOfX2)[] BasisExponents =
        {
            (0, 0), (0, 1), (0, 2), (1, 0), (2, 0), (1, 1), (2, 1), (1, 2),
            (2, 2), (3, 0), (3, 1), (3, 2), (0, 3), (1, 3), (2, 3), (3, 3),
        };

        public IEnumerable<double> GetValues(IEnumerable<DataTwoFact> data)
        {
            var points = data.ToArray();
            if (points.Length < BasisExponents.Length)
                throw new ArgumentOutOfRangeException(nameof(data));

            var (mean1, scale1) = MeanAndScale(points.Select(p => p.X1));
            var (mean2, scale2) = MeanAndScale(points.Select(p => p.X2));

            var design = Matrix<double>.Build.Dense(points.Length, BasisExponents.Length, (row, col) =>
            {
                var x1c = (points[row].X1 - mean1) / scale1;
                var x2c = (points[row].X2 - mean2) / scale2;
                return BasisTerm(BasisExponents[col], x1c, x2c);
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

        /// <summary>
        /// Substitutes x1c=(X1-mean1)/scale1, x2c=(X2-mean2)/scale2 into the fitted
        /// centered-basis polynomial and reads off the coefficient of each raw a0..a15
        /// monomial by repeated symbolic differentiation and evaluation at X1=X2=0 (the
        /// standard Taylor-coefficient trick — valid here because the substituted expression
        /// is itself a polynomial in X1, X2).
        /// </summary>
        private static double[] ConvertToOriginalBasis(
            Vector<double> centeredCoefficients, double mean1, double scale1, double mean2, double scale2)
        {
            SymbolicExpression x1 = SymbolicExpression.Variable("X1");
            SymbolicExpression x2 = SymbolicExpression.Variable("X2");
            SymbolicExpression x1c = (x1 - mean1) / scale1;
            SymbolicExpression x2c = (x2 - mean2) / scale2;

            SymbolicExpression fitted = BasisExponents
                .Select((exponent, k) => (SymbolicExpression)centeredCoefficients[k] * BasisTerm(exponent, x1c, x2c))
                .Aggregate((SymbolicExpression)0, (sum, term) => sum + term);

            return BasisExponents
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

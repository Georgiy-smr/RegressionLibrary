using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearRegression;
using Regression.Two_factor_regression.Interfaces;
using Regression.Two_factor_regression.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Regression.Two_factor_regression.Implements
{
    /// <summary>
    /// Fits a two-factor polynomial over the given <see cref="IBasisExponents"/> by least squares:
    /// minimizes the sum of squared residuals over the given points (QR on a centered/scaled
    /// design matrix, not normal equations — see issue #5). Returns coefficients of the original
    /// monomials X1^i * X2^j in the basis's order. The recommended default; see
    /// <see cref="MinimaxPolynomialSolver"/> for a fit that minimizes the maximum error instead.
    /// </summary>
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

            var basis = new CenteredPolynomialBasis(_basisExponents, points);
            var design = basis.DesignMatrix(points);
            var y = Vector<double>.Build.Dense(points.Length, row => points[row].Y);

            var centeredCoefficients = MultipleRegression.QR(design, y);

            return basis.ConvertToOriginalBasis(centeredCoefficients);
        }
    }
}

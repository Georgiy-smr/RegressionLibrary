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
    /// Fits a two-factor polynomial over the given <see cref="IBasisExponents"/> so that the
    /// <b>maximum absolute error over the given points</b> is as small as possible (minimax / L∞
    /// fit). Returns coefficients of the original monomials X1^i * X2^j in the basis's order —
    /// the same layout as <see cref="PolynomialLeastSquaresSolver"/>, so the two are
    /// interchangeable behind <see cref="IPolynomialFitService"/>.
    /// <para>
    /// <b>How it differs from <see cref="PolynomialLeastSquaresSolver"/>:</b> least squares
    /// minimizes the sum of squared residuals, which spreads the error over all points; minimax
    /// minimizes the single worst residual, which is what
    /// <c>ApproximationCalculationError.GetMax()</c> measures. On the 223/224 calibration
    /// datasets (third order) it lowers the in-sample max error by roughly 30–40%.
    /// </para>
    /// <para>
    /// <b>Prefer it</b> when acceptance is checked strictly at the calibration points themselves
    /// (a max-error bound at exactly those X1/X2 values).
    /// </para>
    /// <para>
    /// <b>Do not prefer it</b> when accuracy is needed between calibration temperatures, or when
    /// the data may contain noisy or suspicious points. Minimax chases the worst points, so a
    /// single outlier pulls the whole fit toward itself; in leave-one-series-out validation it
    /// predicts unseen temperatures worse than least squares. Least squares remains the
    /// recommended default.
    /// </para>
    /// <para>
    /// <b>Iterative and approximate:</b> uses Lawson's iteratively reweighted least squares, which
    /// converges to the minimax solution; the result is typically within a few percent of the
    /// exact minimax optimum (e.g. an exact linear-programming solution), not exactly at it.
    /// </para>
    /// <para>
    /// <b>Data requirement (same as any solver):</b> the number of distinct temperatures (X2)
    /// must exceed the X2-degree of the basis — at least 3 / 4 / 5 for
    /// <see cref="SecondOrderBasisExponents"/> / <see cref="ThirdOrderBasisExponents"/> /
    /// <see cref="FourthOrderBasisExponents"/>. With fewer, the high X2-power terms are
    /// undetermined, and the in-sample error will not reveal it.
    /// </para>
    /// </summary>
    public class MinimaxPolynomialSolver : IPolynomialFitService
    {
        /// <summary>Number of iterations over which relativeTolerance improvement is required.</summary>
        private const int StallWindow = 200;

        private readonly (int PowerOfX1, int PowerOfX2)[] _basisExponents;
        private readonly int _maxIterations;
        private readonly double _relativeTolerance;

        /// <param name="basisExponents">Polynomial basis and output coefficient order.</param>
        /// <param name="maxIterations">Upper bound on Lawson iterations (each one is a weighted QR solve).</param>
        /// <param name="relativeTolerance">
        /// Early stop: iteration ends once the best max error has improved by no more than this
        /// fraction over the last 200 iterations. 0 disables early stopping.
        /// </param>
        public MinimaxPolynomialSolver(
            IBasisExponents basisExponents, int maxIterations = 1000, double relativeTolerance = 1e-6)
        {
            if (maxIterations < 1)
                throw new ArgumentOutOfRangeException(nameof(maxIterations));
            if (relativeTolerance < 0 || double.IsNaN(relativeTolerance))
                throw new ArgumentOutOfRangeException(nameof(relativeTolerance));

            _basisExponents = basisExponents.ToArray();
            _maxIterations = maxIterations;
            _relativeTolerance = relativeTolerance;
        }

        public IEnumerable<double> GetValues(IEnumerable<DataTwoFact> data)
        {
            var points = data.ToArray();
            if (points.Length < _basisExponents.Length)
                throw new ArgumentOutOfRangeException(nameof(data));

            var basis = new CenteredPolynomialBasis(_basisExponents, points);
            var design = basis.DesignMatrix(points);
            var y = Vector<double>.Build.Dense(points.Length, row => points[row].Y);

            var best = LawsonIteration(design, y);

            // The only symbolic conversion of the whole fit, done once on the best iterate.
            return basis.ConvertToOriginalBasis(best);
        }

        private Vector<double> LawsonIteration(Matrix<double> design, Vector<double> y)
        {
            int n = y.Count;
            var weights = Vector<double>.Build.Dense(n, 1.0 / n);

            Vector<double>? best = null;
            double bestMaxError = double.PositiveInfinity;
            double windowStartMaxError = double.PositiveInfinity;

            for (int iteration = 0; iteration < _maxIterations; iteration++)
            {
                // Numeric only: centered coefficients and the numeric design matrix. Converting to
                // the original monomial basis (symbolic, slow) must stay out of this loop — it runs
                // exactly once, after the loop, on the best iterate.
                var sqrtWeights = weights.PointwiseSqrt();
                var weightedDesign = Matrix<double>.Build.DiagonalOfDiagonalVector(sqrtWeights) * design;
                var coefficients = MultipleRegression.QR(weightedDesign, y.PointwiseMultiply(sqrtWeights));
                if (!IsFinite(coefficients))
                    break;

                var absResiduals = (y - design * coefficients).PointwiseAbs();
                var maxError = absResiduals.Maximum();
                if (double.IsNaN(maxError))
                    break;

                // Lawson's max error is not monotonic in the iteration count: keep the best.
                if (maxError < bestMaxError)
                {
                    best = coefficients;
                    bestMaxError = maxError;
                }

                if (maxError == 0)
                    break;

                if (_relativeTolerance > 0 && (iteration + 1) % StallWindow == 0)
                {
                    if (windowStartMaxError - bestMaxError <= _relativeTolerance * windowStartMaxError)
                        break;
                    windowStartMaxError = bestMaxError;
                }

                weights = weights.PointwiseMultiply(absResiduals);
                var sum = weights.Sum();
                if (!(sum > 0) || double.IsInfinity(sum))
                    break;
                weights /= sum;
                if (!IsFinite(weights))
                    break;
            }

            // The first iteration is an unweighted least squares solve; it can only fail to be
            // finite on degenerate data, in which case the plain QR result is the honest answer.
            return best ?? MultipleRegression.QR(design, y);
        }

        private static bool IsFinite(Vector<double> vector) => vector.All(double.IsFinite);
    }
}

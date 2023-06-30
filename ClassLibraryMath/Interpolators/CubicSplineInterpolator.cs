namespace Regression.Interpolators
{
    /// <summary>
    /// Класс работы с интерполированием кубическими сплайнами
    /// </summary>
    public class CubicSplineInterpolator
    {
        #region Private Members

        /// <summary>
        /// Coefficients a of the cubic spline
        /// </summary>
        private double[] _aCoeffs;

        /// <summary>
        /// Coefficients b of the cubic spline
        /// </summary>
        private double[] _bCoeffs;

        /// <summary>
        /// Coefficients c of the cubic spline
        /// </summary>
        private double[] _cCoeffs;

        /// <summary>
        /// Coefficients d the cubic spline
        /// </summary>
        private double[] _dCoeffs;

        #endregion

        #region Public Methods

        /// <summary>
        /// Function returns coefficients of Cubic spline
        /// </summary>
        public List<double[]> CalcCoeffs(int n, double[] x, double[] y)
        {
            n--;

            // Initialize
            _aCoeffs = new double[n];

            _aCoeffs = y.Take(n).ToArray();

            _bCoeffs = new double[n];
            _cCoeffs = new double[n];
            _dCoeffs = new double[n];

            double[] h = new double[n];
            double[] A = new double[n];
            var c = new double[n + 1];

            double[] l = new double[n + 1];
            double[] u = new double[n + 1];
            double[] z = new double[n + 1];

            // Step 1
            for (int i = 0; i <= n - 1; i++)
            {
                h[i] = x[i + 1] - x[i];
            }


            // Step 2
            for (int i = 1; i <= n - 1; ++i)
            {
                A[i] = 3 * (y[i + 1] - y[i]) / h[i] - 3 * (y[i] - y[i - 1]) / h[i - 1];
            }

            // Step 3
            l[0] = 1;
            u[0] = 0;
            z[0] = 0;

            // Step 4
            for (int i = 1; i <= n - 1; ++i)
            {
                l[i] = 2 * (x[i + 1] - x[i - 1]) - h[i - 1] * u[i - 1];
                u[i] = h[i] / l[i];
                z[i] = (A[i] - h[i - 1] * z[i - 1]) / l[i];
            }

            // Step 5
            l[n] = 1;
            z[n] = 0;
            c[n] = 0;

            // Step 6
            for (int j = n - 1; j >= 0; --j)
            {
                c[j] = z[j] - u[j] * c[j + 1];
                _bCoeffs[j] = (y[j + 1] - y[j]) / h[j] - h[j] * (c[j + 1] + 2 * c[j]) / 3;
                _dCoeffs[j] = (c[j + 1] - c[j]) / (3 * h[j]);
            }

            _cCoeffs = c.Take(n).ToArray();

            return new List<double[]> { _aCoeffs, _bCoeffs, _cCoeffs, _dCoeffs };
        }

        #endregion
    }
}

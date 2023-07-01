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
        /// <summary>
        /// Input Value
        /// </summary>
        private double[] _X;

        #endregion

        #region Public Methods

        /// <summary>
        /// Function returns coefficients of Cubic spline
        /// </summary>
        public List<double[]> CalcCoeffs(int n, double[] x, double[] y)
        {
            _X = x;

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
        /// <summary>
        /// The function returns the correction value by the coefficients of Cubic spline
        /// </summary>
        /// <param name="NotCorValue"></param>
        /// <returns></returns>
        public double CorValue(double NotCorValue)
        {
            if (_X != null)
            {
            int NumerPoly = NumPoly(NotCorValue);
            // Step 2:
            double corrValue = _aCoeffs[NumerPoly] +
             _bCoeffs[NumerPoly] * Math.Pow((NotCorValue - _X[NumerPoly]), 1)+
             _cCoeffs[NumerPoly] * Math.Pow((NotCorValue - _X[NumerPoly]), 2)+
             _dCoeffs[NumerPoly] * Math.Pow((NotCorValue - _X[NumerPoly]), 3);
            return corrValue;
            }
            else return 0.0;
        }
        /// <summary>
        /// Function return numer of polynom in dependence in input value.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        private int NumPoly(double Value)
        {
            int N = 0;
            int CountPoly = _X.Length - 1;
            // Step 1: under point ?
            if (Value <= _X[0]) { N = 0; return N; }

            //  : over point ?
            else if (Value >= _X[CountPoly])
            { N = CountPoly; return N - 1; }

            //  : between?
            else
            {
                for (int i = 0; i < CountPoly; ++i)
                    if (Value >= _X[i] && Value <= _X[i + 1])
                        N = i;
            }
            return N;
        }

        #endregion

    }
}

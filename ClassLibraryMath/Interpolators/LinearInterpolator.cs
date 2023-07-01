namespace Regression.Interpolators
{
    /// <summary>
    /// Класс работы с кусочно-линейной интерполяцией
    /// </summary>
    public class LinearInterpolator
    {
        #region Private Members

        /// <summary>
        /// Coefficients offset of Sectionally Linear Interpolation
        /// </summary>
        private double[] _bCoeffs;

        /// <summary>
        /// Coefficients slop of Sectionally Linear Interpolation
        /// </summary>
        private double[] _kCoeffs;

        private double[] _X;

        #endregion

        #region Public Methods

        /// <summary>
        /// Function returns coefficients of Sectionally Linear Interpolation
        /// </summary>
        /// <param name="n"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public List<double[]> CalcCoeffs(int n, double[] x, double[] y)
        {
            _X=x;

            _kCoeffs = new double[n - 1];

            // Рассчет коэффициентов k
            for (int i = 0; i < n - 1; i++)
                _kCoeffs[i] = (y[i + 1] - y[i]) / (x[i + 1] - x[i]);

            _bCoeffs = new double[n - 1];

            // Рассчет коэффициентов b
            for (int i = 0; i < n - 1; i++)
                _bCoeffs[i] = y[i] - _kCoeffs[i] * x[i];

            return new List<double[]> { _kCoeffs, _bCoeffs};
        }
        /// <summary>
        /// The function returns the correction value by the coefficients of Sectionally Linear Interpolation
        /// </summary>
        /// <param name="NotCorValue"></param>
        /// <returns></returns>
        public double CorValue(double NotCorValue)
        {
            if (_X != null)
            {
                int NumerPoly = NumPoly(NotCorValue);

                // Step 2:
                double corrValue = _kCoeffs[NumerPoly] * NotCorValue +
                    _bCoeffs[NumerPoly];
                return corrValue;
            }
            else return 0.0;

        }
        /// <summary>
        /// Function return numer of polynom in dependence in input value
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
            { N = CountPoly; return N-1; }

            //  : between ?
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
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
            _kCoeffs = new double[n - 1];

            // Рассчет коэффициентов k
            for (int i = 0; i < n - 1; i++)
                _kCoeffs[i] = (y[i + 1] - y[i]) / (x[i + 1] - x[i]);

            _bCoeffs = new double[n - 1];

            // Рассчет коэффициентов b
            for (int i = 0; i < n - 1; i++)
                _bCoeffs[i] = y[i] - _kCoeffs[i] * x[i];

            return new List<double[]> { _bCoeffs, _kCoeffs};
        }

        #endregion
    }
}
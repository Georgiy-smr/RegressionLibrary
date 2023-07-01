using Regression.MathService;

namespace Regression.Interpolators
{
    /// <summary>
    /// Класс работы с интерполированием по методу Лагранжа
    /// </summary>
    public class LagrangeInterpolator
    {
        #region Private Members

        /// <summary>
        /// Coefficients of the equation
        /// </summary>
        private double[] _coeffs;

        #endregion

        #region Public methods

        /// <summary>
        /// Function returns coefficients of Lagrange polynomial
        /// </summary>
        /// <param name="n"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public List<double> CalcCoeffs(int n, double[] x, double[] y)
        {
            // Step 1 
            double[,] Y = new double[n, 1];

            for (int i = 0; i < Y.GetUpperBound(0) + 1; i++)
            {
                for (int j = 0; j < Y.GetUpperBound(1) + 1; j++)

                {
                    Y[i, 0] = y[i];
                }
            }

            // Step 2
            double[,] X = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)

                {
                    X[i, j] = Convert.ToDouble(Math.Pow(Convert.ToDouble(x[i]), n - (j + 1)));
                }
            }

            // Step 3
            Gaus gaus = new Gaus();
            _coeffs = gaus.Roots(X, Y);

            return _coeffs.ToList();
        }
        /// <summary>
        /// The function returns the correction value by the coefficients of Lagrange polynomial
        /// </summary>
        /// <param name="NotCorValue"></param>
        /// <returns></returns>
        public double CorValue(double NotCorValue)
        {
            if (_coeffs != null)
            {
            double corrValue=0;
            for (int i = 0; i < _coeffs.Count(); i++)
            {
                {
                    corrValue += _coeffs[i] * Math.Pow(NotCorValue,
                        (_coeffs.Count() - (i + 1)));
                }
            }
            return corrValue;
            }
            else return 0.0;

        }
        #endregion
    }
}

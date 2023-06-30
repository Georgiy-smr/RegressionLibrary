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
        public List<double> CalcRoots(int n, double[] x, double[] y)
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

        #endregion
    }
}

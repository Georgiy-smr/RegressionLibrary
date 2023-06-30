using Regression.MathService;

namespace Regression.Approximators
{
    /// <summary>
    /// Approximation by a Power Polynomial
    /// </summary>
    public class Approximator
    {
        #region Private Members

        /// <summary>
        /// Coefficients of the equation
        /// </summary>
        public double[] _coeffs;

        #endregion

        #region Public Methods

        /// <summary>
        /// Function returns coefficients of Power polynomial
        /// </summary>
        /// <param name="n"></param>
        /// <param name="pow"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public List<double> CalcCoeffs(int n, int pow, double[] x, double[] y)
        {
            // Step 1
            double[,] Y = new double[pow + 1, 1];
            for (int i = 0; i < Y.GetUpperBound(0) + 1; i++)
            {
                for (int j = 0; j < Y.GetUpperBound(1) + 1; j++)

                {
                    for (int I = 0; I < n; I++)
                    {
                        {
                            Y[i, 0] += y[I] * Math.Pow(x[I], i);
                        }
                    }
                }
            }

            // Step 2
            double[,] X = new double[pow + 1, pow + 1];
            for (int i = 0; i < X.GetUpperBound(0) + 1; i++)
            {
                for (int j = 0; j < X.GetUpperBound(1) + 1; j++)
                {
                    for (int I = 0; I < n; I++)
                    {
                        {
                            X[i, j] += Convert.ToDouble(Math.Pow(x[I], i + pow - j));
                        }
                    }
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

using Regression.MathService;
using Regression.Two_factor_regression.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regression.Two_factor_regression.Implements
{
    public class Solver : ISolverSystem
    {
        private readonly Gaus gaus = new Gaus();
        public IEnumerable<double> GetRoots(double[,] X, double[,] Y) => gaus.Roots(X, Y);
    }
}

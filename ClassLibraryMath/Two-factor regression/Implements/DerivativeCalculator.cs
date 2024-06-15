using Regression.Two_factor_regression.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regression.Two_factor_regression.Implements
{
    public class DerivativeCalculator : IDerivatives
    {
        public IEnumerable<string[]> Calculate(IPolynomialExpression expression)
        {
            foreach (var variable in expression.Variables)
            {
                yield return expression.Function.
                    Differentiate(variable).
                    RationalSimplify(variable).
                    ToString()
                    .Trim()
                    .Split();
            }
        }
    }
}

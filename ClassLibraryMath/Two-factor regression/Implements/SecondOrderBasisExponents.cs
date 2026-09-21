using Regression.Two_factor_regression.Interfaces;
using System.Collections;
using System.Collections.Generic;

namespace Regression.Two_factor_regression.Implements
{
    public sealed class SecondOrderBasisExponents : IBasisExponents
    {
        private static readonly (int PowerOfX1, int PowerOfX2)[] Exponents =
        {
            (0, 0), (0, 1), (0, 2), (1, 0), (2, 0), (1, 1), (2, 1), (1, 2), (2, 2),
        };

        public IEnumerator<(int PowerOfX1, int PowerOfX2)> GetEnumerator()
            => ((IEnumerable<(int PowerOfX1, int PowerOfX2)>)Exponents).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

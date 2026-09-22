using Regression.Two_factor_regression.Interfaces;
using System.Collections;
using System.Collections.Generic;

namespace Regression.Two_factor_regression.Implements
{
    public sealed class FourthOrderBasisExponents : IBasisExponents
    {
        private static readonly (int PowerOfX1, int PowerOfX2)[] Exponents =
        {
            (0, 0), (0, 1), (0, 2), (0, 3), (0, 4),
            (1, 0), (1, 1), (1, 2), (1, 3), (1, 4),
            (2, 0), (2, 1), (2, 2), (2, 3), (2, 4),
            (3, 0), (3, 1), (3, 2), (3, 3), (3, 4),
            (4, 0), (4, 1), (4, 2), (4, 3), (4, 4),
        };

        public IEnumerator<(int PowerOfX1, int PowerOfX2)> GetEnumerator()
            => ((IEnumerable<(int PowerOfX1, int PowerOfX2)>)Exponents).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

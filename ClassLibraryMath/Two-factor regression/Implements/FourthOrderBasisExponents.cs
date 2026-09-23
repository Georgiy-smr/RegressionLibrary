using Regression.Two_factor_regression.Interfaces;
using System.Collections;
using System.Collections.Generic;

namespace Regression.Two_factor_regression.Implements
{
    public sealed class FourthOrderBasisExponents : IBasisExponents
    {
        private static readonly (int PowerOfX1, int PowerOfX2)[] Exponents =
        {
            // First 16 entries are ThirdOrderBasisExponents in the same order (whose first 9 are
            // SecondOrderBasisExponents), so a lower-order coefficient array is a positional prefix.
            (0, 0), (0, 1), (0, 2), (1, 0), (2, 0), (1, 1), (2, 1), (1, 2),
            (2, 2), (3, 0), (3, 1), (3, 2), (0, 3), (1, 3), (2, 3), (3, 3),
            (4, 0), (4, 1), (4, 2), (4, 3),
            (0, 4), (1, 4), (2, 4), (3, 4),
            (4, 4),
        };

        public IEnumerator<(int PowerOfX1, int PowerOfX2)> GetEnumerator()
            => ((IEnumerable<(int PowerOfX1, int PowerOfX2)>)Exponents).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

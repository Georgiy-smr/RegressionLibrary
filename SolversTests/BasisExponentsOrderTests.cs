using Regression.ErrorAnalysis;
using Regression.Two_factor_regression;
using Regression.Two_factor_regression.Implements;
using Regression.Two_factor_regression.Interfaces;

namespace SolversTests;

/// <summary>
/// Locks the term-order invariant from issue #15: each lower-order basis is an exact positional
/// prefix of the next higher-order one, so a shorter coefficient array can be truncated from /
/// zero-padded to a longer one without reassigning coefficients to different terms. Also checks
/// that the hardcoded TwoFactorPolynomialValue9/16/25 evaluators use the same term order as the
/// basis the PolynomialLeastSquaresSolver fits with.
/// </summary>
public class BasisExponentsOrderTests
{
    [Fact]
    public void ThirdOrderStartsWithSecondOrder()
    {
        var second = new SecondOrderBasisExponents().ToArray();
        var third = new ThirdOrderBasisExponents().ToArray();

        Assert.Equal(9, second.Length);
        Assert.Equal(16, third.Length);
        Assert.Equal(second, third.Take(second.Length));
    }

    [Fact]
    public void FourthOrderStartsWithThirdOrder()
    {
        var third = new ThirdOrderBasisExponents().ToArray();
        var fourth = new FourthOrderBasisExponents().ToArray();

        Assert.Equal(25, fourth.Length);
        Assert.Equal(third, fourth.Take(third.Length));
    }

    [Fact]
    public void FourthOrderIsTheFullFiveByFiveGrid()
    {
        var fourth = new FourthOrderBasisExponents().ToArray();
        var grid = from i in Enumerable.Range(0, 5) from j in Enumerable.Range(0, 5) select (i, j);

        Assert.Equal(fourth.Length, fourth.Distinct().Count());
        Assert.True(grid.ToHashSet().SetEquals(fourth));
    }

    public static IEnumerable<object[]> Bases => new[]
    {
        new object[] { new SecondOrderBasisExponents() },
        new object[] { new ThirdOrderBasisExponents() },
        new object[] { new FourthOrderBasisExponents() },
    };

    [Theory]
    [MemberData(nameof(Bases))]
    public void PolynomialValueMatchesBasisTermOrder(IBasisExponents basis)
    {
        var exponents = basis.ToArray();
        var point = new DataTwoFact { X1 = 1.3, X2 = -0.7 };

        for (int k = 0; k < exponents.Length; k++)
        {
            var coefficients = new double[exponents.Length];
            coefficients[k] = 1;
            var expected = Math.Pow(point.X1, exponents[k].PowerOfX1) * Math.Pow(point.X2, exponents[k].PowerOfX2);

            var actual = new TwoFactorPolynomialValue(coefficients, point).Value();

            Assert.True(Math.Abs(expected - actual) < 1e-12, $"Term {k} {exponents[k]}: expected {expected}, got {actual}");
        }
    }
}

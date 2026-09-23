using MathNet.Symbolics;
using Regression.Two_factor_regression;
using Regression.Two_factor_regression.Implements;
using Regression.Two_factor_regression.Interfaces;

namespace SolversTests;

/// <summary>
/// ExpressionCreator.CreateFourthOrderPolynomialExpression (issue #16): the symbolic
/// normal-equations path at fourth order, through both solvers (Gaus and MathNet), plus a
/// cross-check that its a0..a24 term order is the same as FourthOrderBasisExponents' (the
/// design-matrix path used by PolynomialLeastSquaresSolver).
/// </summary>
public class FourthOrderPolynomialExpressionTests
{
    private static readonly (int PowerOfX1, int PowerOfX2)[] Exponents = new FourthOrderBasisExponents().ToArray();

    // Distinct, sign-alternating values so a swapped pair of terms can't go unnoticed.
    private static readonly double[] KnownCoefficients =
        Enumerable.Range(0, 25).Select(k => (k % 2 == 0 ? 1 : -1) * (0.5 + 0.1 * k)).ToArray();

    private static double Evaluate(double[] coefficients, double x1, double x2)
        => Exponents.Select((e, k) => coefficients[k] * Math.Pow(x1, e.PowerOfX1) * Math.Pow(x2, e.PowerOfX2)).Sum();

    // 7x7 grid on [-1, 1]^2 (49 points), exact Y from the known polynomial.
    private static readonly List<DataTwoFact> SyntheticData =
        (from i in Enumerable.Range(0, 7)
         from j in Enumerable.Range(0, 7)
         let x1 = -1 + i / 3.0
         let x2 = -1 + j / 3.0
         select new DataTwoFact { X1 = x1, X2 = x2, Y = Evaluate(KnownCoefficients, x1, x2) }).ToList();

    public static IEnumerable<object[]> Solvers => new[]
    {
        new object[] { "Gaus", new Solver() },
        new object[] { "MathNet", new SolverMathNet() },
    };

    [Theory]
    [MemberData(nameof(Solvers))]
    public void RecoversKnownCoefficients(string name, ISolverSystem solver)
    {
        var sut = new ApproximationService(solver, new RowParser(), new DerivativeCalculator());

        var coefficients = sut.GetValues(SyntheticData.CreateFourthOrderPolynomialExpression()).ToArray();

        Assert.Equal(25, coefficients.Length);
        var maxError = coefficients.Select((c, k) => Math.Abs(c - KnownCoefficients[k])).Max();
        Assert.True(maxError < 1e-6, $"{name}: max coefficient error {maxError}");
    }

    [Fact]
    public void TermOrderMatchesFourthOrderBasisExponents()
    {
        // 25 copies of one point with Y = 0, so Function(a) = 25 * p(a)^2. With X1, X2 > 0 every
        // term is positive and all 25 term values are distinct, so evaluating at each unit vector
        // e_k pins down exactly which monomial a_k multiplies.
        const double x1 = 1.3, x2 = 0.7;
        var data = Enumerable.Repeat(new DataTwoFact { X1 = x1, X2 = x2, Y = 0 }, 25);
        var expression = data.CreateFourthOrderPolynomialExpression();
        var variables = expression.Variables.Select(v => v.ToString()).ToArray();

        Assert.Equal(Exponents.Length, variables.Length);
        for (int k = 0; k < Exponents.Length; k++)
        {
            var unit = new double[Exponents.Length];
            unit[k] = 1;
            var expected = 25 * Math.Pow(Evaluate(unit, x1, x2), 2);

            var actual = Evaluate(expression, variables, unit);

            Assert.True(Math.Abs(expected - actual) < 1e-9 * expected, $"a{k} {Exponents[k]}: expected {expected}, got {actual}");
        }

        // Same check with one full coefficient vector.
        var expectedFull = 25 * Math.Pow(Evaluate(KnownCoefficients, x1, x2), 2);
        var actualFull = Evaluate(expression, variables, KnownCoefficients);
        Assert.True(Math.Abs(expectedFull - actualFull) < 1e-9 * expectedFull, $"expected {expectedFull}, got {actualFull}");
    }

    [Fact]
    public void RejectsFewerThan25Points()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SyntheticData.Take(24).CreateFourthOrderPolynomialExpression());
    }

    private static double Evaluate(IPolynomialExpression expression, string[] variables, double[] values)
        => expression.Function.Evaluate(variables
            .Select((name, k) => (name, value: (FloatingPoint)values[k]))
            .ToDictionary(p => p.name, p => p.value)).RealValue;
}

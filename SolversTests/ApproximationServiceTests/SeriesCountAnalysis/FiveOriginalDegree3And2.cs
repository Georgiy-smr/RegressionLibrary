using Regression.Two_factor_regression;
using Regression.Two_factor_regression.Implements;
using Regression.Two_factor_regression.Interfaces;
using Regression.ErrorAnalysis;
using System.Collections;

namespace SolversTests;

/// <summary>
/// P^0-3 x T^0-2 basis (12 terms: (P,T) in {0,1,2,3} x {0,1,2}) — the a0..a15 terms from
/// ThirdOrderBasisExponents with T-power (PowerOfX2) &lt;= 2, i.e. every term except the 4
/// T^3 ones.
/// </summary>
internal sealed class Degree3And2BasisExponents : IBasisExponents
{
    private static readonly (int PowerOfX1, int PowerOfX2)[] Exponents =
    {
        (0, 0), (0, 1), (0, 2), (1, 0), (2, 0), (1, 1), (2, 1), (1, 2), (2, 2), (3, 0), (3, 1), (3, 2),
    };

    public IEnumerator<(int PowerOfX1, int PowerOfX2)> GetEnumerator()
        => ((IEnumerable<(int PowerOfX1, int PowerOfX2)>)Exponents).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Experiment A (issue #10): does dropping T (X2, temperature) to degree 2 instead of 3 — 12
/// of the 16 a0..a15 terms, the remaining 4 (all T^3 terms) pinned to exactly 0.0 in the
/// array sent to the sensor firmware — let FiveOriginal fit within the 0.003 tolerance,
/// without regressing ThreeOriginal/ThreeOriginalAndTwoFake?
///
/// GetMax() per dataset (reduced basis, zero-padded to 16 slots), tolerance 0.003:
///   FiveOriginal            = 0.01068628510194003    (FAIL, worse than the 16-term 0.0067532704376560559)
///   ThreeOriginal           = 0.0012325412373286326  (PASS, essentially unchanged from the 16-term 0.0012097178459100633)
///   ThreeOriginalAndTwoFake = 0.0012278872301578758  (PASS, essentially unchanged from the 16-term 0.0012323117431662922)
///
/// ThreeOriginal/ThreeOriginalAndTwoFake don't regress here (dropping the four T^3 terms costs
/// them essentially nothing), but FiveOriginal still fails -- and gets worse, not better, than
/// the full 16-term basis. Not the fix either. See FiveOriginalDegree3And1.cs for the P^0-3 x
/// T^0-1 variant, and the PR description for the full comparison table and conclusion.
/// </summary>
public class FiveOriginalDegree3And2
{
    private static double[] Fit(List<DataTwoFact> data)
    {
        var sut = new PolynomialLeastSquaresSolver(new Degree3And2BasisExponents());
        var reducedCoefficients = sut.GetValues(data);
        return ReducedBasisExpander.ExpandToFullBasis(reducedCoefficients, new Degree3And2BasisExponents());
    }

    [Fact]
    public void FiveOriginal_MaxErrorIsBelowTolerance()
    {
        var coefficients = Fit(SeriesCountAnalysisData.FiveOriginal);
        var error = new ApproximationCalculationError(coefficients, SeriesCountAnalysisData.FiveOriginal).GetMax();

        Assert.True(error < 0.003);
    }

    [Fact]
    public void ThreeOriginal_MaxErrorIsBelowTolerance()
    {
        var coefficients = Fit(SeriesCountAnalysisData.ThreeOriginal);
        var error = new ApproximationCalculationError(coefficients, SeriesCountAnalysisData.ThreeOriginal).GetMax();

        Assert.True(error < 0.003);
    }

    [Fact]
    public void ThreeOriginalAndTwoFake_MaxErrorIsBelowTolerance()
    {
        var coefficients = Fit(SeriesCountAnalysisData.ThreeOriginalAndTwoFake);
        var error = new ApproximationCalculationError(coefficients, SeriesCountAnalysisData.ThreeOriginalAndTwoFake).GetMax();

        Assert.True(error < 0.003);
    }
}

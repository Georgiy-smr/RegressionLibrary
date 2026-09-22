using Regression.Two_factor_regression;
using Regression.Two_factor_regression.Implements;
using Regression.Two_factor_regression.Interfaces;
using Regression.ErrorAnalysis;
using System.Collections;

namespace SolversTests;

/// <summary>
/// P^0-3 x T^0-1 basis (8 terms: (P,T) in {0,1,2,3} x {0,1}) — the a0..a15 terms from
/// ThirdOrderBasisExponents with T-power (PowerOfX2) &lt;= 1, i.e. every term except the T^2
/// and T^3 ones.
/// </summary>
internal sealed class Degree3And1BasisExponents : IBasisExponents
{
    private static readonly (int PowerOfX1, int PowerOfX2)[] Exponents =
    {
        (0, 0), (0, 1), (1, 0), (2, 0), (1, 1), (2, 1), (3, 0), (3, 1),
    };

    public IEnumerator<(int PowerOfX1, int PowerOfX2)> GetEnumerator()
        => ((IEnumerable<(int PowerOfX1, int PowerOfX2)>)Exponents).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Experiment A (issue #10): does dropping T (X2, temperature) to degree 1 instead of 3 — 8
/// of the 16 a0..a15 terms, the rest pinned to exactly 0.0 in the array sent to the sensor
/// firmware — let FiveOriginal fit within the 0.003 tolerance, without regressing
/// ThreeOriginal/ThreeOriginalAndTwoFake?
///
/// GetMax() per dataset (reduced basis, zero-padded to 16 slots), tolerance 0.003:
///   FiveOriginal            = 0.014283320496161878  (FAIL, worse than the 16-term 0.0067532704376560559)
///   ThreeOriginal           = 0.013589666118122068  (FAIL, worse than the 16-term 0.0012097178459100633)
///   ThreeOriginalAndTwoFake = 0.010489509187863177  (FAIL, worse than the 16-term 0.0012323117431662922)
///
/// All three regress relative to the full 16-term basis -- dropping T to degree 1 does not
/// fix FiveOriginal, and makes ThreeOriginal/ThreeOriginalAndTwoFake noticeably worse too.
/// Not the fix. See FiveOriginalDegree3And2.cs for the P^0-3 x T^0-2 variant, and the PR
/// description for the full comparison table and conclusion.
/// </summary>
public class FiveOriginalDegree3And1
{
    private static double[] Fit(List<DataTwoFact> data)
    {
        var sut = new PolynomialLeastSquaresSolver(new Degree3And1BasisExponents());
        var reducedCoefficients = sut.GetValues(data);
        return ReducedBasisExpander.ExpandToFullBasis(reducedCoefficients, new Degree3And1BasisExponents());
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

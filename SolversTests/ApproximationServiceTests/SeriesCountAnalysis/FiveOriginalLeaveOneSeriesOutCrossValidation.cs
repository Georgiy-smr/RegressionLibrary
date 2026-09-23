using Regression.Two_factor_regression;
using Regression.Two_factor_regression.Implements;
using Regression.Two_factor_regression.Interfaces;
using Regression.ErrorAnalysis;

namespace SolversTests;

/// <summary>
/// Leave-one-series-out cross-validation on FiveOriginal: fit on 4 of the 5 temperature
/// series (44 points) and measure error on the 5th series (11 points), which the fit never
/// saw. Repeated for all 5 held-out choices, at both third order (16 coeff) and fourth order
/// (25 coeff, see FiveOriginalFourthOrder.cs for the in-sample result this follows up on).
///
/// This is the actual test of whether the fourth-order model generalizes to an
/// uncalibrated temperature or just fits the specific 55 in-sample points more closely
/// because it has more free coefficients — the real-world equivalent of "calibrate on these
/// temperature runs, then operate at a temperature you didn't separately calibrate for".
///
/// ANSWER: it doesn't generalize — it overfits badly, confirming the concern raised for
/// PR #13. Worst-case held-out (absolute) error per series:
///
/// | Held-out series (11 pts each) | 3rd order (16 coeff) | 4th order (25 coeff) |
/// |---|---|---|
/// | 0 | 0.061997378767657096 | 10.397481214720756 |
/// | 1 | 0.017371500707412224 | 0.934272088095895 |
/// | 2 | 0.012126808146305734 | 0.585607128906247 |
/// | 3 | 0.020815958818140246 | 2.1845280576125674 |
/// | 4 | 0.08614862646088284 | 13.395025863312185 |
///
/// Third order's worst held-out case (0.086) is degraded from its in-sample GetMax()
/// (0.0068, see FiveOriginal.cs) but stays in the same order of magnitude. Fourth order's
/// worst held-out case (13.4) is nearly 4 orders of magnitude worse than its in-sample
/// GetMax() (0.0023, see FiveOriginalFourthOrder.cs) — the in-sample pass from PR #13 was
/// the model absorbing per-series noise, not learning anything that holds up on a series it
/// wasn't fit on. The 0.1 ceiling below is chosen to sit just above third order's observed
/// worst case, making the two orders' generalization gap explicit rather than hiding it
/// behind a loose blow-up guard; fourth order is intentionally left red against it — see
/// CLAUDE.md's "don't loosen tolerances just to make a red test pass".
/// </summary>
public class FiveOriginalLeaveOneSeriesOutCrossValidation
{
    // FiveOriginal is 5 contiguous series of 11 points each (see SeriesCountAnalysisData).
    private static readonly (int Start, int Count)[] SeriesRanges =
    {
        (0, 11), (11, 11), (22, 11), (33, 11), (44, 11),
    };

    public static IEnumerable<object[]> Series =>
        Enumerable.Range(0, SeriesRanges.Length).Select(i => new object[] { i });

    [Theory]
    [MemberData(nameof(Series))]
    public void ThirdOrder_HeldOutSeriesError(int heldOutIndex)
    {
        var error = HeldOutSeriesMaxError(heldOutIndex, new ThirdOrderBasisExponents());
        Assert.True(error < 0.1, $"Series {heldOutIndex} held-out error: {error}");
    }

    [Theory]
    [MemberData(nameof(Series))]
    public void FourthOrder_HeldOutSeriesError(int heldOutIndex)
    {
        var error = HeldOutSeriesMaxError(heldOutIndex, new FourthOrderBasisExponents());
        Assert.True(error < 0.1, $"Series {heldOutIndex} held-out error: {error}");
    }

    [Fact]
    public void ThirdOrder_MaxErrorAcrossAllHeldOutSeries()
    {
        var maxError = Enumerable.Range(0, SeriesRanges.Length)
            .Select(i => HeldOutSeriesMaxError(i, new ThirdOrderBasisExponents()))
            .Max();
        Assert.True(maxError < 0.1, $"Worst-case held-out error: {maxError}");
    }

    [Fact]
    public void FourthOrder_MaxErrorAcrossAllHeldOutSeries()
    {
        var maxError = Enumerable.Range(0, SeriesRanges.Length)
            .Select(i => HeldOutSeriesMaxError(i, new FourthOrderBasisExponents()))
            .Max();
        Assert.True(maxError < 0.1, $"Worst-case held-out error: {maxError}");
    }

    private static double HeldOutSeriesMaxError(int heldOutIndex, IBasisExponents basis)
    {
        var all = SeriesCountAnalysisData.FiveOriginal;
        var (holdOutStart, holdOutCount) = SeriesRanges[heldOutIndex];

        var trainingData = all
            .Where((_, i) => i < holdOutStart || i >= holdOutStart + holdOutCount)
            .ToList();
        var heldOutData = all.Skip(holdOutStart).Take(holdOutCount).ToList();

        var coefficients = new PolynomialLeastSquaresSolver(basis).GetValues(trainingData).ToArray();

        return heldOutData
            .Select(point => Math.Abs(point.Y - new TwoFactorPolynomialValue(coefficients, point).Value()))
            .Max();
    }
}

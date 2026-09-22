using Regression.Two_factor_regression;
using Regression.Two_factor_regression.Implements;
using Regression.ErrorAnalysis;

namespace SolversTests;

/// <summary>
/// Follow-up on issue #10: is FiveOriginal's excess residual error concentrated in the 2
/// added real series (18C, 24.5C), or spread evenly across all 5? Fits FiveOriginal with the
/// existing baseline PolynomialLeastSquaresSolver(ThirdOrderBasisExponents) (the same fit
/// ApproximationServiceTests/FiveOriginal.cs uses, GetMax()=0.0067532704376560559 overall),
/// then evaluates TwoFactorPolynomialValue per point and groups the 55 absolute errors by
/// series (nearest of the 5 nominal code_T setpoints below, not assumed index ranges).
///
/// code_T (X2)      Temperature   In ThreeOriginal?
///   -709.6729        15C          yes
///   -710.187         18C          no (added in FiveOriginal)
///   -710.7041        21C          yes
///   -711.298         24.5C        no (added in FiveOriginal)
///   -711.8979        28C          yes
///
/// Per-series max/mean absolute error (n=11 each):
///   15C     max=0.0015870069752708105  mean=0.0010355116547696821
///   18C     max=0.0067532704376560559  mean=0.0039662928882206409
///   21C     max=0.0062112733564134714  mean=0.0052890242001204513
///   24.5C   max=0.0042207168552081953  mean=0.0030970957560857537
///   28C     max=0.0017930922860216469  mean=0.00073885600724372068
///
/// This does NOT support the "the 2 added series specifically are bad measurements" theory:
/// 21C is one of the 3 ORIGINAL series (also in ThreeOriginal, which passes at
/// GetMax()=0.0012097178459100633 when fit alone) and has the highest mean error of all 5
/// here, higher than either added series. The actual pattern is by temperature position, not
/// original-vs-added: the two extreme-temperature series (15C, 28C) fit clearly better than
/// the three more central ones (18C, 21C, 24.5C) -- min max-error among {18C,21C,24.5C}
/// (0.0042) is more than 2x max max-error among {15C,28C} (0.0018), and every mean error in
/// the middle trio exceeds every mean error at the extremes by a similar margin. See the PR
/// description and the issue #10 comment for the interpretation.
/// </summary>
public class FiveOriginalPerSeriesResidualBreakdown
{
    private static readonly (string Label, double NominalX2)[] Series =
    {
        ("15C", -709.6729),
        ("18C", -710.187),
        ("21C", -710.7041),
        ("24.5C", -711.298),
        ("28C", -711.8979),
    };

    private static string ClosestSeriesLabel(double x2)
        => Series.OrderBy(s => Math.Abs(s.NominalX2 - x2)).First().Label;

    [Fact]
    public void ExtremeTemperatureSeriesFitClearlyBetterThanMiddleSeries()
    {
        var sut = new PolynomialLeastSquaresSolver(new ThirdOrderBasisExponents());
        var coefficients = sut.GetValues(SeriesCountAnalysisData.FiveOriginal).ToArray();

        var perPoint = SeriesCountAnalysisData.FiveOriginal
            .Select(d => (
                Label: ClosestSeriesLabel(d.X2),
                Error: Math.Abs(d.Y - new TwoFactorPolynomialValue(coefficients, d).Value())))
            .ToList();

        var perSeries = perPoint
            .GroupBy(p => p.Label)
            .ToDictionary(g => g.Key, g => (Max: g.Max(p => p.Error), Mean: g.Average(p => p.Error), Count: g.Count()));

        Assert.Equal(5, perSeries.Count);
        Assert.All(perSeries.Values, s => Assert.Equal(11, s.Count));

        var extremes = new[] { "15C", "28C" };
        var middle = new[] { "18C", "21C", "24.5C" };

        var worstExtremeMax = extremes.Max(label => perSeries[label].Max);
        var bestMiddleMax = middle.Min(label => perSeries[label].Max);

        // The weakest of the 3 middle-temperature series still clearly out-errors the worst
        // of the 2 extreme-temperature series -- a real, measured gap, not noise.
        Assert.True(bestMiddleMax > worstExtremeMax * 2,
            $"expected every middle-temperature series' max error to clearly exceed every " +
            $"extreme-temperature series' max error; bestMiddleMax={bestMiddleMax:G6} " +
            $"worstExtremeMax={worstExtremeMax:G6}");
    }
}

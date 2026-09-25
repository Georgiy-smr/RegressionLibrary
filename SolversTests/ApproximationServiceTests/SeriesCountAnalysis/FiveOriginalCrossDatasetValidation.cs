using MathNet.Numerics.LinearAlgebra;
using Regression.Two_factor_regression;
using Regression.Two_factor_regression.Implements;
using Regression.Two_factor_regression.Interfaces;
using Regression.ErrorAnalysis;

namespace SolversTests;

/// <summary>
/// Issue #10 follow-up: ThreeOriginal and ThreeOriginalAndTwoFake pass the 0.003 tolerance,
/// but only when checked against the points they were fit on. Here the models built from them
/// are checked against all 55 real points of FiveOriginal, including the 18C and 24.5C series
/// neither model ever saw. Per-point absolute error |Y - TwoFactorPolynomialValue| is grouped
/// by series (nearest nominal code_T, as in FiveOriginalPerSeriesResidualBreakdown).
///
/// Models (all PolynomialLeastSquaresSolver):
///   A = ThreeOriginal + ThirdOrderBasisExponents (16 terms)
///   B = ThreeOriginal + Degree3And2BasisExponents (12 terms, zero-padded to 16 via ReducedBasisExpander)
///   C = ThreeOriginalAndTwoFake + ThirdOrderBasisExponents (16 terms)
///   FiveOriginal = FiveOriginal + ThirdOrderBasisExponents (16 terms), reference row only --
///     it was fit on all 5 series, so its numbers are in-sample everywhere.
///
/// Per-series max / mean absolute error on FiveOriginal (n=11 each; * = series the model never saw):
///
/// | Model        | cond(centered design) | 15C             | 18C              | 21C             | 24.5C            | 28C             | Overall max |
/// |--------------|-----------------------|-----------------|------------------|-----------------|------------------|-----------------|-------------|
/// | A            | 57760                 | 0.00030/0.00015 | *0.3647/0.1628   | 0.00121/0.00031 | *0.4458/0.2008   | 0.00117/0.00040 | 0.4458      |
/// | B            | 20.0                  | 0.00037/0.00020 | *0.01573/0.01228 | 0.00123/0.00032 | *0.00334/0.00201 | 0.00111/0.00039 | 0.01573     |
/// | C            | 43.1                  | 0.00037/0.00020 | *0.01574/0.01229 | 0.00123/0.00032 | *0.00334/0.00199 | 0.00111/0.00039 | 0.01574     |
/// | FiveOriginal | 43.1                  | 0.00159/0.00104 | 0.00675/0.00397  | 0.00621/0.00529 | 0.00422/0.00310  | 0.00179/0.00074 | 0.00675     |
///
/// Every A/B/C overall and unseen-series test below is red, and is left red on purpose (see
/// CLAUDE.md: don't loosen tolerances). They document that "3 series pass" was an in-sample
/// result.
///
/// Three separate effects, which should not be confused with each other:
///
/// 1. Identifiability of the T^3 terms (model A only, a model artifact). With k distinct
///    temperatures a polynomial in T is determined only up to degree k-1. ThreeOriginal has 3,
///    so the 4 T^3 columns are fit only through the jitter of code_T inside each series
///    (0.0034-0.0044 wide, against ~1.1 between neighbouring series). The singular value
///    in that direction is tiny, which shows up as cond = 57760 against 20 for B. That is not
///    large enough to hurt MathNet's QR in double precision, which is why the C# numbers match
///    an independent numpy lstsq estimate to 3 digits. The fit is numerically stable but
///    statistically meaningless: the T^3 coefficients are fit to jitter. On its own points A
///    looks fine (&lt;= 0.0012). Between calibration temperatures it is off by up to 0.45,
///    about 150x the tolerance.
///
/// 2. The real error between calibration temperatures (B and C). Dropping T^3 (B) or padding
///    with synthetic series generated from a 2nd-degree fit of the same 3 real series (C) gives
///    the same model: predictions differ by at most 4.8e-5 across all 55 points. The synthetic
///    series add no information, and C's cond (43.1) only looks healthy because the fake points
///    fill the design matrix, not because T^3 is actually measured. Both miss 18C by 0.0157
///    (mean 0.0123, a nearly uniform offset) and 24.5C by 0.0033. That is the error a
///    3-temperature calibration really has at a temperature it never saw.
///
/// 3. Per-series offsets (FiveOriginal's 0.0068, in-sample). With all 5 series measured, the
///    remaining misfit is per-series offsets of alternating sign (mean Y - fit per series:
///    +1.0, -4.0, +5.3, -3.1, +0.7 x10^-3 across 15..28C) that a smooth cubic in T cannot
///    follow. This is separate from both effects above.
///
/// Comparison: B/C's worst unseen-series error (0.0157) is 2.3x FiveOriginal's in-sample
/// 0.0068. So FiveOriginal is not the worse strategy. It is the only dataset that exposes the
/// error between calibration temperatures at all. The fair out-of-sample comparison for
/// FiveOriginal is FiveOriginalLeaveOneSeriesOutCrossValidation: holding out 18C gives 0.017
/// and holding out 24.5C gives 0.021, the same order as B/C's 0.0157 / 0.0033.
/// </summary>
public class FiveOriginalCrossDatasetValidation
{
    private const double Tolerance = 0.003;

    private static readonly (string Label, double NominalX2)[] Series =
    {
        ("15C", -709.6729),
        ("18C", -710.187),
        ("21C", -710.7041),
        ("24.5C", -711.298),
        ("28C", -711.8979),
    };

    // Series present in FiveOriginal but not measured in ThreeOriginal / ThreeOriginalAndTwoFake.
    private static readonly string[] UnseenSeries = { "18C", "24.5C" };

    private static readonly string[] ModelNames = { "A", "B", "C" };

    public static IEnumerable<object[]> Models => ModelNames.Select(m => new object[] { m });

    public static IEnumerable<object[]> ModelsAndUnseenSeries =>
        from model in ModelNames
        from series in UnseenSeries
        select new object[] { model, series };

    [Theory]
    [MemberData(nameof(Models))]
    public void OverallMaxErrorOnFiveOriginalIsBelowTolerance(string model)
    {
        var perSeries = PerSeriesErrorOnFiveOriginal(model);
        Assert.Equal(5, perSeries.Count);
        Assert.All(perSeries.Values, s => Assert.Equal(11, s.Count));

        var error = perSeries.Values.Max(s => s.Max);
        Assert.True(error < Tolerance, $"Model {model}: max error on FiveOriginal = {error:R}");
    }

    [Theory]
    [MemberData(nameof(ModelsAndUnseenSeries))]
    public void UnseenSeriesMaxErrorIsBelowTolerance(string model, string series)
    {
        var error = PerSeriesErrorOnFiveOriginal(model)[series].Max;
        Assert.True(error < Tolerance, $"Model {model}, unseen series {series}: max error = {error:R}");
    }

    [Fact]
    public void ThreeSeriesSixteenTermDesignIsFarWorseConditionedThanTwelveTerm()
    {
        var condA = CenteredDesignConditionNumber("A");
        var condB = CenteredDesignConditionNumber("B");
        var condC = CenteredDesignConditionNumber("C");

        // T^3 is only fit through code_T jitter inside each series in A.
        Assert.True(condA > 100 * Math.Max(condB, condC),
            $"cond A={condA:G6}, B={condB:G6}, C={condC:G6}");
    }

    [Fact]
    public void ModelsBAndCAgreeOnFiveOriginal()
    {
        var b = Fit("B");
        var c = Fit("C");

        var maxDifference = SeriesCountAnalysisData.FiveOriginal
            .Max(d => Math.Abs(new TwoFactorPolynomialValue(b, d).Value() - new TwoFactorPolynomialValue(c, d).Value()));

        // Synthetic series from a 2nd-degree fit of the 3 real ones add no information.
        Assert.True(maxDifference < Tolerance / 10, $"max |B - C| = {maxDifference:R}");
    }

    private static (List<DataTwoFact> Training, IBasisExponents Basis) ModelDefinition(string model) => model switch
    {
        "A" => (SeriesCountAnalysisData.ThreeOriginal, new ThirdOrderBasisExponents()),
        "B" => (SeriesCountAnalysisData.ThreeOriginal, new Degree3And2BasisExponents()),
        "C" => (SeriesCountAnalysisData.ThreeOriginalAndTwoFake, new ThirdOrderBasisExponents()),
        _ => throw new ArgumentOutOfRangeException(nameof(model)),
    };

    private static double[] Fit(string model)
    {
        var (training, basis) = ModelDefinition(model);
        var coefficients = new PolynomialLeastSquaresSolver(basis).GetValues(training);
        return ReducedBasisExpander.ExpandToFullBasis(coefficients, basis);
    }

    private static string ClosestSeriesLabel(double x2)
        => Series.OrderBy(s => Math.Abs(s.NominalX2 - x2)).First().Label;

    private static Dictionary<string, (double Max, double Mean, int Count)> PerSeriesErrorOnFiveOriginal(string model)
    {
        var coefficients = Fit(model);
        return SeriesCountAnalysisData.FiveOriginal
            .Select(d => (
                Label: ClosestSeriesLabel(d.X2),
                Error: Math.Abs(d.Y - new TwoFactorPolynomialValue(coefficients, d).Value())))
            .GroupBy(p => p.Label)
            .ToDictionary(g => g.Key, g => (g.Max(p => p.Error), g.Average(p => p.Error), g.Count()));
    }

    /// <summary>
    /// 2-norm condition number of the design matrix as PolynomialLeastSquaresSolver builds it
    /// (each variable centered on its mean and divided by its population std).
    /// </summary>
    private static double CenteredDesignConditionNumber(string model)
    {
        var (training, basis) = ModelDefinition(model);
        var points = training.ToArray();
        var exponents = basis.ToArray();
        var (mean1, scale1) = MeanAndScale(points.Select(p => p.X1));
        var (mean2, scale2) = MeanAndScale(points.Select(p => p.X2));

        var design = Matrix<double>.Build.Dense(points.Length, exponents.Length, (row, col) =>
            Math.Pow((points[row].X1 - mean1) / scale1, exponents[col].PowerOfX1)
            * Math.Pow((points[row].X2 - mean2) / scale2, exponents[col].PowerOfX2));
        return design.ConditionNumber();
    }

    private static (double Mean, double Scale) MeanAndScale(IEnumerable<double> values)
    {
        var array = values.ToArray();
        var mean = array.Average();
        var scale = Math.Sqrt(array.Select(v => (v - mean) * (v - mean)).Average());
        return (mean, scale == 0 ? 1 : scale);
    }
}

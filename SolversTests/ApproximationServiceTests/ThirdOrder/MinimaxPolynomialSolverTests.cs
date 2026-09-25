using System.Diagnostics;
using Regression.Two_factor_regression;
using Regression.Two_factor_regression.Implements;
using Regression.Two_factor_regression.Interfaces;
using Regression.Two_factor_regression.Interfaces.Services;
using Regression.ErrorAnalysis;

namespace SolversTests;

/// <summary>
/// MinimaxPolynomialSolver (Lawson IRLS, default maxIterations = 1000, relativeTolerance = 1e-6)
/// against PolynomialLeastSquaresSolver on the 223/224 datasets (same data as
/// ApproximationServiceTests223/224).
///
/// In-sample GetMax() actually observed:
///
/// | Dataset / order | Least squares | Minimax (this solver) | Exact minimax (LP) | Minimax / LP |
/// |---|---|---|---|---|
/// | 223, third  | 0.0015240687579307632 | 0.0009458302304778954 | 0.000945824 | 1.000 |
/// | 224, third  | 0.0014365282110873068 | 0.0010397911168169571 | 0.001020655 | 1.019 |
/// | 223, second | 0.012990764246524122  | 0.009605818849141912  | —           | — |
/// | 223, fourth | 0.0011958321036900088 | 0.0009197213053653286 | —           | — |
///
/// Leave-one-series-out, third order (fit on 4 series / 44 points, max error on the held-out
/// 11-point series; series 1..3 are the interior temperatures 12.5 / 25 / 37.5 °C):
///
/// | Held-out series | 223 LSQ | 223 minimax | 224 LSQ | 224 minimax |
/// |---|---|---|---|---|
/// | 0 (edge)     | 0.006511724143308584  | 0.01063063919724172   | 0.012486935700485091  | 0.024572163391468393  |
/// | 1 (12.5 °C)  | 0.0019241618650767123 | 0.0028508335158861087 | 0.003171488090165653  | 0.004819651973960504  |
/// | 2 (25 °C)    | 0.0012608605443169552 | 0.0023665694715759855 | 0.0019627046926311564 | 0.001866798596338981  |
/// | 3 (37.5 °C)  | 0.0013324422033065275 | 0.0016009998315524854 | 0.002809620413708913  | 0.0041791783365141555 |
/// | 4 (edge)     | 0.0042370708537049495 | 0.010722962297457173  | 0.012423373363350532  | 0.013239816361210188  |
///
/// Minimax fits the calibration points better but predicts unseen temperatures worse: it
/// chases the worst points. That trade-off is locked in by the leave-one-series-out test.
/// </summary>
public class MinimaxPolynomialSolverTests
{
    private const double Tolerance = 0.011;
    private const double ExactMinimax223 = 0.000945824;
    private const double ExactMinimax224 = 0.001020655;

    // 5 contiguous temperature series of 11 points each; 1..3 are the interior temperatures.
    private const int SeriesLength = 11;
    private static readonly int[] InteriorSeries = { 1, 2, 3 };

    public static IEnumerable<object[]> ThirdOrderDatasets => new[]
    {
        new object[] { "223", ExactMinimax223 },
        new object[] { "224", ExactMinimax224 },
    };

    [Theory]
    [MemberData(nameof(ThirdOrderDatasets))]
    public void ThirdOrder_MaxErrorNotAboveLeastSquares(string dataset, double _)
    {
        var data = Data(dataset);
        var (leastSquaresError, minimaxError) = MaxErrors(data, new ThirdOrderBasisExponents());

        Assert.True(minimaxError <= leastSquaresError, $"minimax {minimaxError} > least squares {leastSquaresError}");
        Assert.True(minimaxError < Tolerance);
        Assert.True(leastSquaresError < Tolerance);
    }

    [Theory]
    [MemberData(nameof(ThirdOrderDatasets))]
    public void ThirdOrder_MaxErrorWithinFivePercentOfExactMinimax(string dataset, double exactMinimax)
    {
        var data = Data(dataset);
        var minimaxError = new ApproximationCalculationError(
            new MinimaxPolynomialSolver(new ThirdOrderBasisExponents()).GetValues(data), data).GetMax();

        Assert.True(minimaxError <= 1.05 * exactMinimax, $"minimax {minimaxError} vs exact {exactMinimax}");
    }

    public static IEnumerable<object[]> OtherOrders => new[]
    {
        new object[] { new SecondOrderBasisExponents(), 9 },
        new object[] { new FourthOrderBasisExponents(), 25 },
    };

    [Theory]
    [MemberData(nameof(OtherOrders))]
    public void OtherOrders_On223_ReturnFiniteCoefficientsNotWorseThanLeastSquares(IBasisExponents basis, int coefficientCount)
    {
        var data = Data("223");
        var coefficients = new MinimaxPolynomialSolver(basis).GetValues(data).ToArray();
        var (leastSquaresError, minimaxError) = MaxErrors(data, basis);

        Assert.Equal(coefficientCount, coefficients.Length);
        Assert.All(coefficients, c => Assert.True(double.IsFinite(c)));
        Assert.True(minimaxError <= leastSquaresError, $"minimax {minimaxError} > least squares {leastSquaresError}");
    }

    [Fact]
    public void BothSolvers_UsableThroughCommonInterface()
    {
        var data = Data("223");
        var variants = new List<Func<List<DataTwoFact>, IEnumerable<double>>>
        {
            d => new PolynomialLeastSquaresSolver(new ThirdOrderBasisExponents()).GetValues(d),
            d => new MinimaxPolynomialSolver(new ThirdOrderBasisExponents()).GetValues(d),
        };
        IPolynomialFitService[] fits =
        {
            new PolynomialLeastSquaresSolver(new ThirdOrderBasisExponents()),
            new MinimaxPolynomialSolver(new ThirdOrderBasisExponents()),
        };
        ILeastSquaresRegressionService leastSquares = new PolynomialLeastSquaresSolver(new ThirdOrderBasisExponents());

        Assert.All(variants.Select(v => v(data)), c => Assert.Equal(16, c.Count()));
        Assert.All(fits.Select(f => f.GetValues(data)), c => Assert.Equal(16, c.Count()));
        Assert.Equal(16, leastSquares.GetValues(data).Count());
        Assert.IsNotAssignableFrom<ILeastSquaresRegressionService>(fits[1]);
    }

    [Theory]
    [MemberData(nameof(ThirdOrderDatasets))]
    public void LeaveOneSeriesOut_LeastSquaresGeneralizesAtLeastAsWellAtInteriorTemperatures(string dataset, double _)
    {
        var data = Data(dataset);
        var leastSquaresWorst = InteriorSeries.Max(s => HeldOutSeriesMaxError(
            data, s, d => new PolynomialLeastSquaresSolver(new ThirdOrderBasisExponents()).GetValues(d)));
        var minimaxWorst = InteriorSeries.Max(s => HeldOutSeriesMaxError(
            data, s, d => new MinimaxPolynomialSolver(new ThirdOrderBasisExponents()).GetValues(d)));

        Assert.True(leastSquaresWorst <= minimaxWorst, $"least squares {leastSquaresWorst} > minimax {minimaxWorst}");
    }

    [Fact]
    public void TooFewPoints_Throws()
    {
        var data = Data("223").Take(15);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new MinimaxPolynomialSolver(new ThirdOrderBasisExponents()).GetValues(data));
    }

    [Fact]
    public void ExactFitData_ReturnsGeneratingCoefficients()
    {
        var basis = new ThirdOrderBasisExponents().ToArray();
        var expected = basis.Select((_, k) => (k % 3 - 1) * 0.5 + 0.1 * k).ToArray();
        var data = (
            from x1 in Enumerable.Range(0, 11)
            from x2 in Enumerable.Range(-2, 5)
            select new DataTwoFact
            {
                X1 = x1,
                X2 = x2,
                Y = basis.Select((e, k) => expected[k] * Math.Pow(x1, e.PowerOfX1) * Math.Pow(x2, e.PowerOfX2)).Sum(),
            }).ToList();

        var coefficients = new MinimaxPolynomialSolver(new ThirdOrderBasisExponents()).GetValues(data).ToArray();

        Assert.All(coefficients, c => Assert.True(double.IsFinite(c)));
        Assert.All(coefficients.Zip(expected), p => Assert.Equal(p.Second, p.First, 1e-6));
        Assert.True(new ApproximationCalculationError(coefficients, data).GetMax() < 1e-8);
    }

    /// <summary>
    /// Guards against the symbolic conversion to the original basis creeping back into the
    /// iteration loop (that would be ~1000× slower). Observed: ~50 ms for 223 third order vs
    /// ~3 ms for least squares. The least-squares fit runs first to keep JIT/MathNet.Symbolics
    /// warm-up out of the measurement.
    /// </summary>
    [Fact]
    public void ThirdOrderFit_CompletesQuickly()
    {
        var data = Data("223");
        new PolynomialLeastSquaresSolver(new ThirdOrderBasisExponents()).GetValues(data).ToArray();

        var stopwatch = Stopwatch.StartNew();
        new MinimaxPolynomialSolver(new ThirdOrderBasisExponents()).GetValues(data).ToArray();
        stopwatch.Stop();

        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(2), $"Took {stopwatch.Elapsed}");
    }

    private static (double LeastSquares, double Minimax) MaxErrors(List<DataTwoFact> data, IBasisExponents basis)
        => (new ApproximationCalculationError(new PolynomialLeastSquaresSolver(basis).GetValues(data), data).GetMax(),
            new ApproximationCalculationError(new MinimaxPolynomialSolver(basis).GetValues(data), data).GetMax());

    private static double HeldOutSeriesMaxError(
        List<DataTwoFact> data, int heldOutIndex, Func<List<DataTwoFact>, IEnumerable<double>> fit)
    {
        var start = heldOutIndex * SeriesLength;
        var training = data.Where((_, i) => i < start || i >= start + SeriesLength).ToList();
        var coefficients = fit(training).ToArray();

        return data.Skip(start).Take(SeriesLength)
            .Select(point => Math.Abs(point.Y - new TwoFactorPolynomialValue(coefficients, point).Value()))
            .Max();
    }

    private static List<DataTwoFact> Data(string dataset) => dataset switch
    {
        "223" => Data223,
        "224" => Data224,
        _ => throw new ArgumentOutOfRangeException(nameof(dataset)),
    };

    // Same rows as ApproximationServiceTests223._data (223.xlsx, Лист1, A/B/C = X1/X2/Y, rows 2-56).
    private static readonly List<DataTwoFact> Data223 = new()
    {
        new DataTwoFact() { X1 = 2.47246, X2 = -645.5524, Y = 4.003 },
        new DataTwoFact() { X1 = 4.63174, X2 = -645.5073, Y = 11.003 },
        new DataTwoFact() { X1 = 8.02411, X2 = -645.4373, Y = 22.003 },
        new DataTwoFact() { X1 = 11.4149, X2 = -645.3694, Y = 33.004 },
        new DataTwoFact() { X1 = 14.80412, X2 = -645.3033, Y = 44.004 },
        new DataTwoFact() { X1 = 18.19175, X2 = -645.2376, Y = 55.004 },
        new DataTwoFact() { X1 = 21.5776, X2 = -645.1734, Y = 66.004 },
        new DataTwoFact() { X1 = 24.96179, X2 = -645.1099, Y = 77.005 },
        new DataTwoFact() { X1 = 28.34399, X2 = -645.0473, Y = 88.005 },
        new DataTwoFact() { X1 = 31.7244, X2 = -644.9859, Y = 99.006 },
        new DataTwoFact() { X1 = 35.10245, X2 = -644.9258, Y = 110.007 },
        new DataTwoFact() { X1 = 2.6702, X2 = -649.8272, Y = 4.003 },
        new DataTwoFact() { X1 = 4.78345, X2 = -649.7841, Y = 11.003 },
        new DataTwoFact() { X1 = 8.10368, X2 = -649.7178, Y = 22.003 },
        new DataTwoFact() { X1 = 11.42291, X2 = -649.6525, Y = 33.004 },
        new DataTwoFact() { X1 = 14.74107, X2 = -649.5891, Y = 44.004 },
        new DataTwoFact() { X1 = 18.0578, X2 = -649.5266, Y = 55.004 },
        new DataTwoFact() { X1 = 21.37301, X2 = -649.4651, Y = 66.004 },
        new DataTwoFact() { X1 = 24.6869, X2 = -649.4041, Y = 77.005 },
        new DataTwoFact() { X1 = 27.99855, X2 = -649.3453, Y = 88.005 },
        new DataTwoFact() { X1 = 31.31006, X2 = -649.2857, Y = 99.006 },
        new DataTwoFact() { X1 = 34.61871, X2 = -649.2279, Y = 110.007 },
        new DataTwoFact() { X1 = 2.84419, X2 = -654.1574, Y = 4.003 },
        new DataTwoFact() { X1 = 4.91516, X2 = -654.1246, Y = 11.003 },
        new DataTwoFact() { X1 = 8.16945, X2 = -654.0675, Y = 22.003 },
        new DataTwoFact() { X1 = 11.42239, X2 = -654.0099, Y = 33.004 },
        new DataTwoFact() { X1 = 14.67466, X2 = -653.9527, Y = 44.004 },
        new DataTwoFact() { X1 = 17.92564, X2 = -653.8955, Y = 55.004 },
        new DataTwoFact() { X1 = 21.17541, X2 = -653.8392, Y = 66.004 },
        new DataTwoFact() { X1 = 24.42361, X2 = -653.7832, Y = 77.005 },
        new DataTwoFact() { X1 = 27.67093, X2 = -653.7273, Y = 88.005 },
        new DataTwoFact() { X1 = 30.91661, X2 = -653.6714, Y = 99.006 },
        new DataTwoFact() { X1 = 34.16058, X2 = -653.6164, Y = 110.007 },
        new DataTwoFact() { X1 = 3.00703, X2 = -658.6233, Y = 4.003 },
        new DataTwoFact() { X1 = 5.03772, X2 = -658.5915, Y = 11.003 },
        new DataTwoFact() { X1 = 8.22834, X2 = -658.5369, Y = 22.003 },
        new DataTwoFact() { X1 = 11.41823, X2 = -658.4816, Y = 33.004 },
        new DataTwoFact() { X1 = 14.60745, X2 = -658.4255, Y = 44.004 },
        new DataTwoFact() { X1 = 17.79564, X2 = -658.3704, Y = 55.004 },
        new DataTwoFact() { X1 = 20.98334, X2 = -658.3159, Y = 66.004 },
        new DataTwoFact() { X1 = 24.16971, X2 = -658.2613, Y = 77.005 },
        new DataTwoFact() { X1 = 27.35501, X2 = -658.2075, Y = 88.005 },
        new DataTwoFact() { X1 = 30.53933, X2 = -658.1535, Y = 99.006 },
        new DataTwoFact() { X1 = 33.72218, X2 = -658.0999, Y = 110.007 },
        new DataTwoFact() { X1 = 3.1654, X2 = -663.1766, Y = 4.003 },
        new DataTwoFact() { X1 = 5.15679, X2 = -663.1404, Y = 11.003 },
        new DataTwoFact() { X1 = 8.286, X2 = -663.0836, Y = 22.003 },
        new DataTwoFact() { X1 = 11.41489, X2 = -663.0269, Y = 33.004 },
        new DataTwoFact() { X1 = 14.5436, X2 = -662.9696, Y = 44.004 },
        new DataTwoFact() { X1 = 17.6714, X2 = -662.9139, Y = 55.004 },
        new DataTwoFact() { X1 = 20.7988, X2 = -662.8591, Y = 66.004 },
        new DataTwoFact() { X1 = 23.9256, X2 = -662.8049, Y = 77.005 },
        new DataTwoFact() { X1 = 27.05131, X2 = -662.7521, Y = 88.005 },
        new DataTwoFact() { X1 = 30.17673, X2 = -662.6992, Y = 99.006 },
        new DataTwoFact() { X1 = 33.3005, X2 = -662.6468, Y = 110.007 },
    };

    // Same rows as ApproximationServiceTests224._data (224.xlsx, Лист1, A/B/C = X1/X2/Y, rows 2-56).
    private static readonly List<DataTwoFact> Data224 = new()
    {
        new DataTwoFact() { X1 = 2.7027, X2 = -644.7589, Y = 4.003 },
        new DataTwoFact() { X1 = 4.81841, X2 = -644.7155, Y = 11.003 },
        new DataTwoFact() { X1 = 8.14223, X2 = -644.6481, Y = 22.003 },
        new DataTwoFact() { X1 = 11.46457, X2 = -644.5815, Y = 33.004 },
        new DataTwoFact() { X1 = 14.78551, X2 = -644.5168, Y = 44.004 },
        new DataTwoFact() { X1 = 18.10467, X2 = -644.4529, Y = 55.004 },
        new DataTwoFact() { X1 = 21.42234, X2 = -644.3899, Y = 66.004 },
        new DataTwoFact() { X1 = 24.73835, X2 = -644.3284, Y = 77.005 },
        new DataTwoFact() { X1 = 28.05254, X2 = -644.2677, Y = 88.005 },
        new DataTwoFact() { X1 = 31.36506, X2 = -644.2078, Y = 99.006 },
        new DataTwoFact() { X1 = 34.6752, X2 = -644.1488, Y = 110.007 },
        new DataTwoFact() { X1 = 2.86763, X2 = -649.0464, Y = 4.003 },
        new DataTwoFact() { X1 = 4.93818, X2 = -649.005, Y = 11.003 },
        new DataTwoFact() { X1 = 8.19139, X2 = -648.941, Y = 22.003 },
        new DataTwoFact() { X1 = 11.44358, X2 = -648.8778, Y = 33.004 },
        new DataTwoFact() { X1 = 14.69459, X2 = -648.8158, Y = 44.004 },
        new DataTwoFact() { X1 = 17.94443, X2 = -648.7546, Y = 55.004 },
        new DataTwoFact() { X1 = 21.19267, X2 = -648.6941, Y = 66.004 },
        new DataTwoFact() { X1 = 24.43969, X2 = -648.6348, Y = 77.005 },
        new DataTwoFact() { X1 = 27.68525, X2 = -648.5779, Y = 88.005 },
        new DataTwoFact() { X1 = 30.92934, X2 = -648.5195, Y = 99.006 },
        new DataTwoFact() { X1 = 34.17131, X2 = -648.4634, Y = 110.007 },
        new DataTwoFact() { X1 = 3.02186, X2 = -653.3955, Y = 4.003 },
        new DataTwoFact() { X1 = 5.051, X2 = -653.3613, Y = 11.003 },
        new DataTwoFact() { X1 = 8.2392, X2 = -653.3041, Y = 22.003 },
        new DataTwoFact() { X1 = 11.42638, X2 = -653.2472, Y = 33.004 },
        new DataTwoFact() { X1 = 14.61276, X2 = -653.1903, Y = 44.004 },
        new DataTwoFact() { X1 = 17.79794, X2 = -653.1333, Y = 55.004 },
        new DataTwoFact() { X1 = 20.98184, X2 = -653.0777, Y = 66.004 },
        new DataTwoFact() { X1 = 24.16459, X2 = -653.0222, Y = 77.005 },
        new DataTwoFact() { X1 = 27.34637, X2 = -652.9674, Y = 88.005 },
        new DataTwoFact() { X1 = 30.52667, X2 = -652.913, Y = 99.006 },
        new DataTwoFact() { X1 = 33.70535, X2 = -652.8588, Y = 110.007 },
        new DataTwoFact() { X1 = 3.17301, X2 = -657.8646, Y = 4.003 },
        new DataTwoFact() { X1 = 5.16253, X2 = -657.8324, Y = 11.003 },
        new DataTwoFact() { X1 = 8.28844, X2 = -657.7783, Y = 22.003 },
        new DataTwoFact() { X1 = 11.41352, X2 = -657.723, Y = 33.004 },
        new DataTwoFact() { X1 = 14.53808, X2 = -657.6685, Y = 44.004 },
        new DataTwoFact() { X1 = 17.66166, X2 = -657.6139, Y = 55.004 },
        new DataTwoFact() { X1 = 20.78455, X2 = -657.5597, Y = 66.004 },
        new DataTwoFact() { X1 = 23.90652, X2 = -657.506, Y = 77.005 },
        new DataTwoFact() { X1 = 27.02751, X2 = -657.4531, Y = 88.005 },
        new DataTwoFact() { X1 = 30.14749, X2 = -657.4007, Y = 99.006 },
        new DataTwoFact() { X1 = 33.26594, X2 = -657.3482, Y = 110.007 },
        new DataTwoFact() { X1 = 3.32198, X2 = -662.4174, Y = 4.003 },
        new DataTwoFact() { X1 = 5.27291, X2 = -662.3825, Y = 11.003 },
        new DataTwoFact() { X1 = 8.33864, X2 = -662.3275, Y = 22.003 },
        new DataTwoFact() { X1 = 11.40405, X2 = -662.272, Y = 33.004 },
        new DataTwoFact() { X1 = 14.4692, X2 = -662.2175, Y = 44.004 },
        new DataTwoFact() { X1 = 17.53339, X2 = -662.1635, Y = 55.004 },
        new DataTwoFact() { X1 = 20.5971, X2 = -662.1101, Y = 66.004 },
        new DataTwoFact() { X1 = 23.66044, X2 = -662.0574, Y = 77.005 },
        new DataTwoFact() { X1 = 26.723, X2 = -662.0053, Y = 88.005 },
        new DataTwoFact() { X1 = 29.78504, X2 = -661.9543, Y = 99.006 },
        new DataTwoFact() { X1 = 32.84511, X2 = -661.9033, Y = 110.007 },
    };
}

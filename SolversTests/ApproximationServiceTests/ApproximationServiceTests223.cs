using Regression.Two_factor_regression.Implements;
using Regression.Two_factor_regression;
using Regression.Two_factor_regression.Interfaces.Services;
using Regression.ErrorAnalysis;

namespace SolversTests;

public class ApproximationServiceTests223
{
    private List<DataTwoFact> _data = new()
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

    private static readonly double[] MathCadCoefficients =
    {
        18388.907500224206,
        83.3267863173459,
        0.12606686160785655,
        -737.8973304145301,
        -10.096348502901922,
        -3.324339181498154,
        -0.046629299470129,
        -0.00499059714817943,
        -0.0000717526899727,
        0.24378911381962476,
        0.00112189388381146,
        0.00000172074630973,
        0.00006370361954318,
        -0.00000250856109145,
        -0.00000003678932996,
        0.00000000087964539
    };

    /// <summary>
    /// BUG: this test fails because SolverMathNet and Solver (Gaus) both produce coefficients
    /// with a max residual error far above the 0.011 tolerance for the 223 dataset, while the
    /// independently computed MathCad reference coefficients (MathCadCoefficients field) do not.
    /// Tracked in issue #5.
    ///
    /// GetMax() error per source:
    ///   mathNetError = 0.0430808567629839   (FAIL, tolerance 0.011)
    ///   gausError    = 0.22373078245273348  (FAIL, tolerance 0.011)
    ///   mathCadError = 0.001508223661289776 (PASS, tolerance 0.011)
    ///
    /// Coefficient comparison (idx = term in CreateThirdOrderPolynomialExpression, a0 = constant):
    ///
    /// idx    MathNet                    Gaus                       MathCad
    /// a0     161409.05113804847         505775.06827121763         18388.907500224206
    /// a1     739.333203478237           2318.1218505178367         83.3267863173459
    /// a2     1.1290076313509756         3.541600059190415          0.12606686160785655
    /// a3     -37322.97747918869         -73092.88912280153         -737.8973304145301
    /// a4     2391.9073739582336         2626.4660369192025         -10.096348502901922
    /// a5     -171.14624877819676        -335.0419673345773         -3.324339181498154
    /// a6     10.972532569607008         12.036441461540273         -0.046629299470129
    /// a7     -0.26158640274216516       -0.5118935111310263        -0.00499059714817943
    /// a8     0.01677746155826782        0.018385728906721244       -0.0000717526899727
    /// a9     -43.38270883070656         -23.914352361248177        0.24378911381962476
    /// a10    -0.1990254701014542        -0.10948934889237993       0.00112189388381146
    /// a11    -0.00030433854536385794    -0.00016708589956332197    0.00000172074630973
    /// a12    0.000574795283533706       0.001803648982314731       0.00006370361954318
    /// a13    -0.00013327812897499284    -0.0002606972464241017     -0.00000250856109145
    /// a14    8.550714420897579E-06      9.360950778093841E-06      -0.00000003678932996
    /// a15    -1.5511777889159928E-07    -8.498894543825043E-08     0.00000000087964539
    ///
    /// Note the qualitative difference, not just magnitude: MathNet/Gaus put a0..a3 in the tens
    /// to hundreds of thousands while Y ranges only 0..110, and a4's sign even flips between
    /// MathCad (negative) and MathNet/Gaus (positive) — this smells like catastrophic
    /// cancellation from an ill-conditioned normal-equations matrix, not just "needs more
    /// precision". See issue #5 for the working hypothesis and suggested next steps.
    /// </summary>
    [Fact]
    public void MaxErrorIsBelowTolerance()
    {
        ApproximationService sutMathNet = new ApproximationService(new SolverMathNet(), new RowParser(), new DerivativeCalculator());
        ApproximationService sutGaus = new ApproximationService(new Solver(), new RowParser(), new DerivativeCalculator());
        var pressurePolynimial = _data.CreateThirdOrderPolynomialExpression();
        var mathNet = sutMathNet.GetValues(pressurePolynimial);
        var gause = sutGaus.GetValues(pressurePolynimial);
        var mathCad = MathCadCoefficients;
        string s = string.Join(";", mathNet.Select((x, i) => $"a{i} {x}"));
        string g = string.Join(";", gause.Select((x, i) => $"a{i} {x}"));
        var mathNetError = new ApproximationCalculationError(mathNet, _data).GetMax();
        var gausError = new ApproximationCalculationError(gause, _data).GetMax();
        var mathCadError = new ApproximationCalculationError(mathCad, _data).GetMax();
        Assert.True(mathNetError < 0.011);
        Assert.True(gausError < 0.011);
        Assert.True(mathCadError < 0.011);
    }
}

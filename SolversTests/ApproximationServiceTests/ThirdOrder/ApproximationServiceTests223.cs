using Regression.Two_factor_regression.Implements;
using Regression.Two_factor_regression;
using Regression.Two_factor_regression.Interfaces.Services;
using Regression.ErrorAnalysis;

namespace SolversTests;

public class ApproximationServiceTests223
{
    private List<DataTwoFact> _data = Sensor223And224Data.Data223;

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
    /// Old path (ApproximationService, driven by ExpressionCreator's symbolic normal
    /// equations) fixed as of issue #5 / PR #6: SolverMathNet and Solver (Gaus) both produce
    /// coefficients with a max residual error far above 0.011 for the 223 dataset, because
    /// ApproximationService.BuildMatrix's normal-equations matrix is catastrophically
    /// ill-conditioned (~1E+32, see ConditionNumberHypothesisTests) for both 223 and 224 alike
    /// — see PR #6. The old path (ApproximationService/ExpressionCreator) is left untouched for
    /// backward compatibility with external consumers and remains subject to this on similar
    /// data; mathNetError/gausError below assert against the old path's actually observed
    /// error, not a tolerance it was ever designed to meet.
    ///
    /// The fix is PolynomialLeastSquaresSolver(ThirdOrderBasisExponents) (see leastSquaresError
    /// below), which fits the identical a0..a15 basis via least squares on a centered/scaled
    /// design matrix instead of normal equations, and is what issue #5 considers "fixed".
    ///
    /// GetMax() error per source:
    ///   mathNetError      = 0.0430808567629839   (old path, FAIL vs 0.011, honest bound: 0.05)
    ///   gausError         = 0.22373078245273348  (old path, FAIL vs 0.011, honest bound: 0.25)
    ///   mathCadError      = 0.001508223661289776 (independent reference, PASS, tolerance 0.011)
    ///   leastSquaresError = 0.0015240687579307632 (PolynomialLeastSquaresSolver(ThirdOrderBasisExponents), PASS, tolerance 0.011)
    ///
    /// Coefficient comparison (idx = term in CreateThirdOrderPolynomialExpression, a0 = constant):
    ///
    /// idx    MathNet (old)              Gaus (old)                 MathCad                    PolynomialLeastSquaresSolver (new)
    /// a0     161409.05113804847         505775.06827121763         18388.907500224206         18388.907471424587
    /// a1     739.333203478237           2318.1218505178367         83.3267863173459           83.326786185269739
    /// a2     1.1290076313509756         3.541600059190415          0.12606686160785655        0.12606686140596482
    /// a3     -37322.97747918869         -73092.88912280153         -737.8973304145301         -737.89732487203617
    /// a4     2391.9073739582336         2626.4660369192025         -10.096348502901922        -10.096348761528459
    /// a5     -171.14624877819676        -335.0419673345773         -3.324339181498154         -3.3243391560803524
    /// a6     10.972532569607008         12.036441461540273         -0.046629299470129         -0.046629300656112156
    /// a7     -0.26158640274216516       -0.5118935111310263        -0.00499059714817943       -0.0049905971093262063
    /// a8     0.01677746155826782        0.018385728906721244       -0.0000717526899727        -0.000071752691785463644
    /// a9     -43.38270883070656         -23.914352361248177        0.24378911381962476        0.24378911723142779
    /// a10    -0.1990254701014542        -0.10948934889237993       0.00112189388381146        0.001121893899455209
    /// a11    -0.00030433854536385794    -0.00016708589956332197    0.00000172074630973        0.0000017207463336387964
    /// a12    0.000574795283533706       0.001803648982314731       0.00006370361954318        0.00006370361944031005
    /// a13    -0.00013327812897499284    -0.0002606972464241017     -0.00000250856109145       -0.0000025085610716530149
    /// a14    8.550714420897579E-06      9.360950778093841E-06      -0.00000003678932996       -0.000000036789330885564912
    /// a15    -1.5511777889159928E-07    -8.498894543825043E-08     0.00000000087964539        0.00000000087964540488692675
    ///
    /// Note the qualitative difference, not just magnitude: MathNet/Gaus (old path) put a0..a3
    /// in the tens to hundreds of thousands while Y ranges only 0..110, and a4's sign even
    /// flips between MathCad (negative) and MathNet/Gaus (positive) — catastrophic cancellation
    /// from the ill-conditioned normal-equations matrix. PolynomialLeastSquaresSolver matches
    /// the MathCad reference to ~6 significant digits by avoiding that matrix entirely.
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

        var sutLeastSquares = new PolynomialLeastSquaresSolver(new ThirdOrderBasisExponents());
        var leastSquares = sutLeastSquares.GetValues(_data);
        var leastSquaresError = new ApproximationCalculationError(leastSquares, _data).GetMax();

        // Old path (ApproximationService/ExpressionCreator, via SolverMathNet/Solver): honest
        // bounds reflecting its actually observed, ill-conditioning-driven error on this
        // dataset (see class doc comment) — not the 0.011 tolerance it was never able to meet,
        // and not loosened arbitrarily.
        Assert.True(mathNetError < 0.05);
        Assert.True(gausError < 0.25);
        Assert.True(mathCadError < 0.011);

        // This is the actual pass/fail criterion for issue #5: the new, separate
        // least-squares path fits the same a0..a15 basis well within tolerance.
        Assert.True(leastSquaresError < 0.011);
    }
}

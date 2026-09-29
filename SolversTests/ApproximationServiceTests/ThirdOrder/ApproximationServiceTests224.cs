using Regression.Two_factor_regression.Implements;
using Regression.Two_factor_regression;
using Regression.Two_factor_regression.Interfaces.Services;
using Regression.ErrorAnalysis;

namespace SolversTests;

public class ApproximationServiceTests224
{
    private List<DataTwoFact> _data = Sensor223And224Data.Data224;

    /// <summary>
    /// mathNetError=0.0027569095466457583; "a0 3958,8705012631667;a1 17,633225413610994;a2 0,026366616215831737;a3 86,3114100013865;a4 -42,46205990095279;a5 0,4528766013477425;a6 -0,19516720177561458;a7 0,0007795217201033843;a8 -0,000298974152236497;a9 0,5901607677936288;a10 0,0027111853050081043;a11 4,151413936916086E-06;a12 1,3265388911034727E-05;a13 4,293106059243243E-07;a14 -1,5264608236235387E-07;a15 2,1187515003148477E-09"
    /// gausError=0.0039702098220377735; "a0 1999,3158303640707;a1 8,64182395072958;a2 0,012615011526845965;a3 160,82476276871637;a4 -29,143405019811418;a5 0,7932712188234934;a6 -0,13387921081235968;a7 0,0012978153987179743;a8 -0,00020497051748619914;a9 0,13741968120377507;a10 0,0006295621936236712;a11 9,612735113026666E-07;a12 6,25510900984923E-06;a13 6,923457217566713E-07;a14 -1,0458781333604184E-07;a15 4,891816292371359E-10"
    ///
    /// Added for issue #5 / PR #6: leastSquaresError=0.0014365282110873068 via
    /// PolynomialLeastSquaresSolver(ThirdOrderBasisExponents) (centered/scaled design matrix +
    /// QR, see ApproximationServiceTests223 for the full rationale) — confirms the new path
    /// doesn't regress the dataset the old path already handled fine.
    /// </summary>
    [Fact]
    public void MaxErrorIsBelowTolerance()
    {
        ApproximationService sutMathNet = new ApproximationService(new SolverMathNet(), new RowParser(), new DerivativeCalculator());
        ApproximationService sutGaus = new ApproximationService(new Solver(), new RowParser(), new DerivativeCalculator());
        var pressurePolynimial = _data.CreateThirdOrderPolynomialExpression();
        var mathNet = sutMathNet.GetValues(pressurePolynimial);
        var gause = sutGaus.GetValues(pressurePolynimial);
        string s = string.Join(";", mathNet.Select((x, i) => $"a{i} {x}"));
        string g = string.Join(";", gause.Select((x, i) => $"a{i} {x}"));
        var mathNetError = new ApproximationCalculationError(mathNet, _data).GetMax();
        var gausError = new ApproximationCalculationError(gause, _data).GetMax();

        var sutLeastSquares = new PolynomialLeastSquaresSolver(new ThirdOrderBasisExponents());
        var leastSquares = sutLeastSquares.GetValues(_data);
        var leastSquaresError = new ApproximationCalculationError(leastSquares, _data).GetMax();

        Assert.True(mathNetError < 0.011);
        Assert.True(gausError < 0.011);
        Assert.True(leastSquaresError < 0.011);
    }
}

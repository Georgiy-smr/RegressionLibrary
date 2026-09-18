using Regression.Two_factor_regression.Implements;
using Regression.Two_factor_regression;
using Regression.Two_factor_regression.Interfaces.Services;
using Regression.ErrorAnalysis;

namespace SolversTests;

public class ApproximationServiceTests
{
    private List<DataTwoFact> _data = new()
    {
        new DataTwoFact() { X1 = 32.32060, X2 = 95.40960, Y = 80.0025 },
        new DataTwoFact() { X1 = 33.69090, X2 = 95.44200, Y = 84.003 }, 
        new DataTwoFact() { X1 = 35.06040, X2 = 95.47540, Y = 88.004 }, 
        new DataTwoFact() { X1 = 36.42900, X2 = 95.50540, Y = 92.003 }, 
        new DataTwoFact() { X1 = 37.79580, X2 = 95.53650, Y = 96.003 }, 
        new DataTwoFact() { X1 = 39.16280, X2 = 95.56640, Y = 100.004 }, 
        new DataTwoFact() { X1 = 40.52840, X2 = 95.59610, Y = 104.004 }, 
        new DataTwoFact() { X1 = 41.89270, X2 = 95.62670, Y = 108.004 }, 
        new DataTwoFact() { X1 = 43.25600, X2 = 95.65530, Y = 112.0041 }, 
        new DataTwoFact() { X1 = 44.61780, X2 = 95.68270, Y = 116.004 }, 
        new DataTwoFact() { X1 = 45.97900, X2 = 95.71030, Y = 120.0048 },
        new DataTwoFact() { X1 = 32.17465, X2 = 94.7868923077, Y = 80.0025 }, 
        new DataTwoFact() { X1 = 33.53531, X2 = 94.816778022, Y = 84.0034 }, 
        new DataTwoFact() { X1 = 34.89471, X2 = 94.8469373626, Y = 88.0035 },
        new DataTwoFact() { X1 = 36.25330, X2 = 94.8750230769, Y = 92.0034 },
        new DataTwoFact() { X1 = 37.61062, X2 = 94.90348, Y = 96.0033 },
        new DataTwoFact() { X1 = 38.96741, X2 = 94.9319659341, Y = 100.004 },
        new DataTwoFact() { X1 = 40.32321, X2 = 94.9596901099, Y = 104.004 },
        new DataTwoFact() { X1 = 41.67776, X2 = 94.9876373627, Y = 108.004 },
        new DataTwoFact() { X1 = 43.03096, X2 = 95.0145494506, Y = 112.0041 },
        new DataTwoFact() { X1 = 44.38386, X2 = 95.0414087912, Y = 116.004 },
        new DataTwoFact() { X1 = 45.73471, X2 = 95.0680274725, Y = 120.0048 },
        new DataTwoFact() { X1 = 32.02930, X2 = 94.15650, Y = 80.0025 },
        new DataTwoFact() { X1 = 33.38040, X2 = 94.18430, Y = 84.0034 },
        new DataTwoFact() { X1 = 34.73000, X2 = 94.21200, Y = 88.0035 },
        new DataTwoFact() { X1 = 36.07880, X2 = 94.23860, Y = 92.0034 },
        new DataTwoFact() { X1 = 37.42670, X2 = 94.26510, Y = 96.0033 },
        new DataTwoFact() { X1 = 38.77365, X2 = 94.29230, Y = 100.004 },
        new DataTwoFact() { X1 = 40.11980, X2 = 94.31850, Y = 104.004 },
        new DataTwoFact() { X1 = 41.46480, X2 = 94.34450, Y = 108.004 },
        new DataTwoFact() { X1 = 42.80830, X2 = 94.37010, Y = 112.0041 },
        new DataTwoFact() { X1 = 44.15200, X2 = 94.39630, Y = 116.004 },
        new DataTwoFact() { X1 = 45.49310, X2 = 94.42200, Y = 120.0048 },
        new DataTwoFact() { X1 = 31.86049, X2 = 93.4113298077, Y = 80.0025 },
        new DataTwoFact() { X1 = 33.20053, X2 = 93.4372381410, Y = 84.0034 },
        new DataTwoFact() { X1 = 34.53906, X2 = 93.4630564103, Y = 88.0035 },
        new DataTwoFact() { X1 = 35.87674, X2 = 93.4884647436, Y = 92.0034 },
        new DataTwoFact() { X1 = 37.21374, X2 = 93.5135480769, Y = 96.0033 },
        new DataTwoFact() { X1 = 38.54966, X2 = 93.5394105769, Y = 100.004 },
        new DataTwoFact() { X1 = 39.88474, X2 = 93.5644032051, Y = 104.004 },
        new DataTwoFact() { X1 = 41.21887, X2 = 93.5890230769, Y = 108.004 },
        new DataTwoFact() { X1 = 42.55155, X2 = 93.6135673077, Y = 112.0041 },
        new DataTwoFact() { X1 = 43.88414, X2 = 93.6388480769, Y =116.004 },
        new DataTwoFact() { X1=45.21461, X2=93.66356, Y=120.0048 },
        new DataTwoFact() {X1=31.69250, X2=92.65570, Y=80.0025},
        new DataTwoFact() {X1=33.02159, X2=92.68030, Y=84.0034},
        new DataTwoFact() {X1=34.34945, X2=92.70530, Y=88.0035},
        new DataTwoFact() {X1=35.67632 ,X2=92.73010, Y=92.0034},
        new DataTwoFact() {X1=37.00250 ,X2=92.75470, Y=96.0033},
        new DataTwoFact() {X1=38.32790 ,X2=92.77940, Y=100.004},
        new DataTwoFact() {X1=39.65210 ,X2=92.80380, Y=104.004},
        new DataTwoFact() {X1=40.97564 ,X2=92.82800, Y=108.004},
        new DataTwoFact() {X1=42.29805 ,X2=92.85200, Y=112.0041},
        new DataTwoFact() {X1=43.61911 ,X2=92.87620, Y=116.004},
        new DataTwoFact() {X1=44.93978 ,X2=92.90000, Y=120.0048}
    };
    [Fact]
    public void Test()
    {
        ApproximationService sutMathNet = new ApproximationService(new SolverMathNet(), new RowParser(), new DerivativeCalculator());
        ApproximationService sutGaus = new ApproximationService(new Solver(), new RowParser(), new DerivativeCalculator());

        var pressurePolynimial = _data.CreateThirdOrderPolynomialExpression();
        var mathNet = sutMathNet.GetValues(pressurePolynimial);
        var gause = sutGaus.GetValues(pressurePolynimial);

        string s = string.Join(";", mathNet.Select((x, i) => $"a{i} {x}"));
        string g = string.Join(";", gause.Select((x, i) => $"a{i} {x}"));

        var mathNetError = new ApproximationCalculationError(mathNet, _data);
        var gausError = new ApproximationCalculationError(gause, _data);

        Assert.True(mathNetError.GetMax() > 0);
        Assert.True(gausError.GetMax() > 0);
    }
}
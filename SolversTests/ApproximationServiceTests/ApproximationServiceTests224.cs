using Regression.Two_factor_regression.Implements;
using Regression.Two_factor_regression;
using Regression.Two_factor_regression.Interfaces.Services;
using Regression.ErrorAnalysis;

namespace SolversTests;

public class ApproximationServiceTests224
{
    private List<DataTwoFact> _data = new()
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
        var mathNetError = new ApproximationCalculationError(mathNet, _data);
        var gausError = new ApproximationCalculationError(gause, _data);
        Assert.True(mathNetError.GetMax() < 0.003);
        Assert.True(gausError.GetMax() < 0.003);
    }
}

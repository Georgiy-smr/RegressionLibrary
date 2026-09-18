using Regression.Approximators;
using Regression.ErrorAnalysis;

namespace SolversTests;

public class ApproximatorTests
{
    [Fact]
    public void MaxErrorIsBelowTolerance()
    {
        int n = 5;
        int pow = 2;
        double[] x = { 1, 2, 3, 4, 5 };
        double[] y = { 2.1, 3.9, 9.2, 16.1, 24.8 };

        var coeffs = new Approximator().CalcCoeffs(n, pow, x, y);

        var error = new ApproximationCalculationError(coeffs, x, y);

        Assert.True(error.GetMax() < 0.5);
    }
}

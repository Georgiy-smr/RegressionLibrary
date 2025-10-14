using Regression.Two_factor_regression.Implements;
using Regression.Two_factor_regression.Interfaces;

namespace SolversTests;

public class QRfactorized
{
    [Fact]
    public void Test()
    {
        ISolverSystem sut = new SolverMathNet();

        double[] expected = new double[2] { 1.6, 1.6 };

        double[,] A = {
            { 2, 3 },
            { 1, -1 }
        };

        double[,] b = {
            { 8 },
            { 0 }
        };
        var roots = sut.GetRoots(A, b);

        var errors = roots.Select((x, i) => x - expected[i]);

        Assert.True(errors.All(x => x < 0.00001));
    }

    [Fact]
    public void Test_2()
    {
        ISolverSystem sut = new SolverMathNet();

        double[] expected = new double[2] { 2.071428571428571, -0.6428571428571425 };

        double[,] A = {
            { 60, 16 },
            { 16, 8 }
        };

        double[,] b = {
            { 114 },
            { 28 }
        };
        var roots = sut.GetRoots(A, b);

        var errors = roots.Select((x, i) => x - expected[i]);

        Assert.True(errors.All(x => x < 0.00001));
    }
}
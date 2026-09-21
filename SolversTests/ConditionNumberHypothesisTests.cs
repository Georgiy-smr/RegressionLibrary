using MathNet.Numerics.LinearAlgebra;
using Regression.Two_factor_regression;
using Regression.Two_factor_regression.Implements;

namespace SolversTests;

/// <summary>
/// Diagnostics-only test for issue #5's ill-conditioning hypothesis. Builds the same
/// normal-equations matrix that ApproximationService.BuildMatrix passes to ISolverSystem
/// (i.e. the coefficient matrix of the differentiated sum-of-squared-residuals system, not
/// a raw design matrix) for datasets 223 and 224, and inspects its condition number.
///
/// Uncentered X2 values around -645..-663 raised up to the 3rd power (and cross-multiplied
/// with X1 up to the 3rd power) make the matrix entries span many orders of magnitude, which
/// is the mechanism the hypothesis in issue #5 blames for the catastrophic cancellation seen
/// in ApproximationServiceTests223.MaxErrorIsBelowTolerance().
///
/// Measured condition numbers (MathNet Matrix&lt;double&gt;.ConditionNumber(), 2-norm):
///   cond(223) = 4.56962498431329114E+032
///   cond(224) = 2.06130095652292780E+032
///
/// double has about 15-17 significant decimal digits (machine epsilon ~2.22E-16, i.e.
/// ~1/2.22E-16 = ~4.5E+15 before all significant digits are expected to be lost). Both
/// matrices exceed that threshold by roughly 17 orders of magnitude, so both are
/// catastrophically, hopelessly ill-conditioned in IEEE-754 double precision — not just 223.
///
/// This REFUTES the specific part of the issue #5 hypothesis that 223 is ill-conditioned
/// while 224 is not: 224's condition number is the same order of magnitude as 223's
/// (2.06E+32 vs 4.57E+32, a ratio of only ~2.2x, both about 17 orders of magnitude past
/// where double-precision solving is expected to lose all significant digits), yet 224
/// passes MaxErrorIsBelowTolerance() and 223 does not. Ill-conditioning of the normal-
/// equations matrix from uncentered, high-power X2 terms is real and severe for BOTH
/// datasets — arguably both should fail on conditioning grounds alone — so condition number
/// by itself does not explain why 223's solve is qualitatively worse than 224's. Given how
/// far past the precision-loss threshold both matrices sit, the fix in BuildMatrix likely
/// needs to avoid forming XᵀX explicitly at all (e.g. center/scale X1 and X2 before raising
/// to high powers, or solve via QR/SVD on the design matrix directly instead of the normal
/// equations) rather than trying to nurse the current normal-equations approach back to a
/// tolerable condition number. See issue #5 for discussion and next steps.
/// </summary>
public class ConditionNumberHypothesisTests
{
    private static readonly List<DataTwoFact> _data223 = new()
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

    private static readonly List<DataTwoFact> _data224 = new()
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

    /// <summary>
    /// Mirrors ApproximationService.BuildInputMatrix (private) to obtain the exact
    /// normal-equations matrix that gets handed to ISolverSystem.GetRoots, without touching
    /// ApproximationService itself.
    /// </summary>
    private static Matrix<double> BuildNormalEquationsMatrix(IEnumerable<DataTwoFact> data)
    {
        var expression = data.CreateThirdOrderPolynomialExpression();
        var countExpr = expression.Variables.Count();
        var polysList = new DerivativeCalculator().Calculate(expression).ToList();
        var rowsParser = new RowParser();
        double[,] matrix = new double[countExpr, countExpr];
        for (int i = 0; i < countExpr; i++)
        {
            var currentRow = rowsParser.GetRowInput(polysList[i], countExpr).ToArray();
            for (int j = 0; j < countExpr; j++)
                matrix[i, j] += currentRow[j];
        }
        return Matrix<double>.Build.DenseOfArray(matrix);
    }

    [Fact]
    public void BothDatasetsAreCatastrophicallyIllConditionedByASimilarMargin()
    {
        var cond223 = BuildNormalEquationsMatrix(_data223).ConditionNumber();
        var cond224 = BuildNormalEquationsMatrix(_data224).ConditionNumber();

        // Machine epsilon for double is ~2.22E-16, so a condition number beyond ~1/epsilon
        // (~4.5E+15) means a direct solve is expected to lose all significant digits.
        const double doublePrecisionLossThreshold = 1.0 / 2.22e-16;

        Assert.True(cond223 > doublePrecisionLossThreshold,
            $"cond(223)={cond223:E} expected to exceed the double-precision loss threshold {doublePrecisionLossThreshold:E}");
        Assert.True(cond224 > doublePrecisionLossThreshold,
            $"cond(224)={cond224:E} expected to exceed the double-precision loss threshold {doublePrecisionLossThreshold:E}");

        // If ill-conditioning alone explained why 223 fails MaxErrorIsBelowTolerance() while
        // 224 passes, we'd expect cond(223) to be orders of magnitude larger than cond(224).
        // It isn't: the ratio is small (~2.2x here), so this assertion is what actually
        // refutes the "223 is uniquely ill-conditioned" framing of the issue #5 hypothesis —
        // both datasets are equally, hopelessly ill-conditioned in double precision.
        var ratio = cond223 / cond224;
        Assert.True(ratio is > 0.1 and < 10,
            $"expected cond(223)/cond(224) to be within one order of magnitude, was {ratio:E} " +
            $"(cond223={cond223:E}, cond224={cond224:E})");
    }
}

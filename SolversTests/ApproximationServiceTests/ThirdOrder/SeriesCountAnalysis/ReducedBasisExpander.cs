using Regression.Two_factor_regression.Implements;
using Regression.Two_factor_regression.Interfaces;

namespace SolversTests;

/// <summary>
/// Expands coefficients fitted against a reduced basis (fewer terms than the full 16-term
/// a0..a15 one) into a full 16-slot array in ThirdOrderBasisExponents' index order, zero-
/// filling every slot the reduced basis doesn't cover. Mirrors the real production
/// constraint that the sensor firmware always receives exactly 16 coefficients in that fixed
/// order, and sending 0.0 for an unused slot is valid and already how it works today — the
/// dropped terms are pinned to exactly 0, not fit freely.
/// </summary>
internal static class ReducedBasisExpander
{
    public static double[] ExpandToFullBasis(IEnumerable<double> reducedCoefficients, IBasisExponents reducedBasis)
    {
        var fullExponents = new ThirdOrderBasisExponents().ToList();
        var reducedExponents = reducedBasis.ToList();
        var reducedValues = reducedCoefficients.ToList();

        var full = new double[fullExponents.Count];
        for (int i = 0; i < reducedExponents.Count; i++)
        {
            var index = fullExponents.IndexOf(reducedExponents[i]);
            full[index] = reducedValues[i];
        }
        return full;
    }
}

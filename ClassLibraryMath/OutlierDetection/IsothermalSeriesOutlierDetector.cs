using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearRegression;
using Regression.Two_factor_regression;

namespace Regression.OutlierDetection;

/// <summary>
/// Finds mis-loaded points (wrong, forgotten or extra dead weight) in one isothermal
/// calibration series, right after it is recorded, while the point can still be re-measured.
///
/// Within one temperature Y = f(X1) is smooth and nearly linear, so a 2nd-degree polynomial in
/// X1 describes a clean series down to the sensor noise. The detector searches exhaustively for
/// the smallest set of points whose removal leaves the rest on such a curve with every residual
/// ≤ τ, while every removed point misses the curve by more than τ. Exhaustive search, not
/// "drop the largest residual and refit": least squares smears an error at an end point, or a
/// shifted tail, over the whole series, so the largest residual is often at the wrong point.
///
/// Input convention (as used by the library's consumers): X1 = pressure code, X2 = temperature
/// code (ignored — within a series it only drifts with pressure), Y = reference pressure. All Y of
/// a series must be on one scale (e.g. gauge pressure, vacuum negative); the detector knows
/// nothing about units or reference gauges.
/// </summary>
public class IsothermalSeriesOutlierDetector
{
    private const int Degree = 2;
    private const int MaxOutliersCap = 4;
    private const int MinimumPoints = Degree + 3;

    private readonly double _relativeTolerance;

    /// <param name="relativeTolerance">
    /// Noise threshold τ as a fraction of the series' pressure range:
    /// τ = relativeTolerance × (Ymax − Ymin). The default 5·10⁻⁵ is about 1.9× the worst
    /// 2nd-degree residual seen on real series of sensors 223/224/235. τ is the sensor noise,
    /// not "half a weight": with τ that large the curve absorbs an error at an end point.
    /// Errors of about 50τ are found exactly; near 10τ several errors may come back as
    /// <see cref="AmbiguousPoint"/>, or on unevenly spaced series even as a wrong
    /// <see cref="Outlier"/> set.
    /// </param>
    public IsothermalSeriesOutlierDetector(double relativeTolerance = 5e-5)
    {
        if (!(relativeTolerance > 0))
            throw new ArgumentOutOfRangeException(nameof(relativeTolerance), relativeTolerance, "Must be positive.");
        _relativeTolerance = relativeTolerance;
    }

    /// <summary>
    /// Returns the suspicious points of one isothermal series, sorted by <see cref="SuspiciousPoint.Index"/>.
    /// Empty — the series is clean. All <see cref="Outlier"/> — the minimal outlier set is unique.
    /// All <see cref="AmbiguousPoint"/> — several minimal sets fit equally well; their union is returned.
    /// </summary>
    /// <exception cref="ArgumentException">Fewer than 5 points, or all X1 equal.</exception>
    /// <exception cref="SeriesNotResolvableException">
    /// No set of at most min(4, n − 5) points explains the deviations.
    /// </exception>
    public IReadOnlyList<SuspiciousPoint> GetSuspiciousPoints(IEnumerable<DataTwoFact> series)
    {
        if (series is null) throw new ArgumentNullException(nameof(series));
        var points = series.ToArray();
        if (points.Length < MinimumPoints)
            throw new ArgumentException($"A series needs at least {MinimumPoints} points, got {points.Length}.", nameof(series));
        if (points.All(p => p.X1 == points[0].X1))
            throw new ArgumentException("All X1 values of the series are equal.", nameof(series));

        var n = points.Length;
        var maxOutliers = Math.Min(MaxOutliersCap, n - MinimumPoints);
        var tolerance = _relativeTolerance * (points.Max(p => p.Y) - points.Min(p => p.Y));
        var vandermonde = CenteredVandermonde(points);
        var y = Vector<double>.Build.Dense(points.Select(p => p.Y).ToArray());

        for (var k = 0; k <= maxOutliers; k++)
        {
            var accepted = new List<(int[] Removed, double[] Residuals)>();
            foreach (var removed in Combinations(n, k))
            {
                var residuals = ResidualsWithout(removed, vandermonde, y, points);
                if (residuals is not null && IsAccepted(removed, residuals, tolerance))
                    accepted.Add((removed, residuals));
            }

            if (accepted.Count == 1)
            {
                var (removed, residuals) = accepted[0];
                return removed.Select(i => (SuspiciousPoint)new Outlier(i, points[i], residuals[i])).ToList();
            }
            if (accepted.Count > 1)
            {
                return accepted.SelectMany(a => a.Removed).Distinct().OrderBy(i => i)
                    .Select(i => (SuspiciousPoint)new AmbiguousPoint(i, points[i])).ToList();
            }
        }

        throw new SeriesNotResolvableException(maxOutliers, tolerance);
    }

    private static bool IsAccepted(int[] removed, double[] residuals, double tolerance)
    {
        for (var i = 0; i < residuals.Length; i++)
        {
            var isRemoved = Array.IndexOf(removed, i) >= 0;
            var fits = Math.Abs(residuals[i]) <= tolerance;
            if (isRemoved == fits) return false;
        }
        return true;
    }

    /// <summary>
    /// Fits the polynomial to every point not in <paramref name="removed"/> and returns the
    /// residual Y − f(X1) of every point of the series, or null if the kept points cannot
    /// determine the polynomial (fewer than Degree + 1 distinct X1).
    /// </summary>
    private static double[]? ResidualsWithout(int[] removed, Matrix<double> vandermonde, Vector<double> y, DataTwoFact[] points)
    {
        var kept = Enumerable.Range(0, points.Length).Where(i => Array.IndexOf(removed, i) < 0).ToArray();
        if (kept.Select(i => points[i].X1).Distinct().Count() <= Degree) return null;

        var x = Matrix<double>.Build.Dense(kept.Length, Degree + 1, (row, col) => vandermonde[kept[row], col]);
        var coefficients = MultipleRegression.QR(x, Vector<double>.Build.Dense(kept.Length, row => y[kept[row]]));
        return (y - vandermonde * coefficients).ToArray();
    }

    /// <summary>
    /// Vandermonde matrix over centered/scaled X1, so the fit stays well-conditioned
    /// (no normal equations — see issue #5). Coefficients are never converted back to the
    /// original basis: only predictions and residuals are needed.
    /// </summary>
    private static Matrix<double> CenteredVandermonde(DataTwoFact[] points)
    {
        var mean = points.Average(p => p.X1);
        var scale = Math.Sqrt(points.Average(p => (p.X1 - mean) * (p.X1 - mean)));
        return Matrix<double>.Build.Dense(points.Length, Degree + 1,
            (row, col) => Math.Pow((points[row].X1 - mean) / scale, col));
    }

    private static IEnumerable<int[]> Combinations(int n, int k)
    {
        var indices = Enumerable.Range(0, k).ToArray();
        while (true)
        {
            yield return (int[])indices.Clone();
            var i = k - 1;
            while (i >= 0 && indices[i] == n - k + i) i--;
            if (i < 0) yield break;
            indices[i]++;
            for (var j = i + 1; j < k; j++) indices[j] = indices[j - 1] + 1;
        }
    }
}

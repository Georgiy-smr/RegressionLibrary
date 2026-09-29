using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.LinearRegression;
using Regression.Two_factor_regression;

namespace Regression.OutlierDetection;

public class IsothermalSeriesOutlierDetector
{
    private const int Degree = 2;
    private const int MaxOutliersCap = 4;
    private const int MinimumPoints = Degree + 3;
    private const double MaxLeverage = 0.9;

    private readonly double _halfPermissibleErrorFraction;

    public IsothermalSeriesOutlierDetector(double accuracyClassPercent = 0.01)
    {
        if (!(accuracyClassPercent > 0))
            throw new ArgumentOutOfRangeException(nameof(accuracyClassPercent), accuracyClassPercent, "The accuracy class must be positive.");
        _halfPermissibleErrorFraction = accuracyClassPercent / 100 / 2;
    }

    public IReadOnlyList<SuspiciousPoint> GetSuspiciousPoints(IEnumerable<DataTwoFact> series)
    {
        if (series is null) throw new ArgumentNullException(nameof(series));
        var points = series.ToArray();
        if (points.Length < MinimumPoints)
            throw new ArgumentException($"A series needs at least {MinimumPoints} points, got {points.Length}.", nameof(series));
        if (points.All(p => p.X1 == points[0].X1))
            throw new ArgumentException("All X1 values of the series are equal.", nameof(series));
        if (points.All(p => p.Y == points[0].Y))
            throw new ArgumentException("All Y values of the series are equal.", nameof(series));

        var n = points.Length;
        var maxOutliers = Math.Min(MaxOutliersCap, n - MinimumPoints);
        var tolerance = _halfPermissibleErrorFraction * (points.Max(p => p.Y) - points.Min(p => p.Y)) * Math.Abs(TheilSenSlope(points));
        var vandermonde = CenteredVandermonde(points);
        var codes = Vector<double>.Build.Dense(points.Select(p => p.X1).ToArray());

        for (var k = 0; k <= maxOutliers; k++)
        {
            var accepted = new List<(int[] Removed, double[] Residuals, int[] Unverifiable)>();
            foreach (var removed in Combinations(n, k))
            {
                var residuals = ResidualsWithout(removed, vandermonde, codes, points);
                if (residuals is not null && IsAccepted(removed, residuals, tolerance))
                    accepted.Add((removed, residuals, Unverifiable(removed, vandermonde)));
            }

            if (accepted.Count == 0)
                continue;

            if (accepted.Count == 1 && accepted[0].Unverifiable.Length == 0)
            {
                var (removed, residuals, _) = accepted[0];
                return removed.Select(i => (SuspiciousPoint)new Outlier(i, points[i], residuals[i])).ToList();
            }

            return accepted.SelectMany(a => a.Removed.Concat(a.Unverifiable)).Distinct().OrderBy(i => i)
                .Select(i => (SuspiciousPoint)new AmbiguousPoint(i, points[i])).ToList();
        }

        throw new SeriesNotResolvableException(maxOutliers, tolerance);
    }

    private static int[] Unverifiable(int[] removed, Matrix<double> vandermonde)
    {
        var kept = KeptIndices(removed, vandermonde.RowCount);
        var q = KeptRows(kept, vandermonde).QR(QRMethod.Thin).Q;
        return kept.Where((_, row) => q.Row(row).DotProduct(q.Row(row)) > MaxLeverage).ToArray();
    }

    private static int[] KeptIndices(int[] removed, int count)
        => Enumerable.Range(0, count).Where(i => Array.IndexOf(removed, i) < 0).ToArray();

    private static Matrix<double> KeptRows(int[] kept, Matrix<double> vandermonde)
        => Matrix<double>.Build.Dense(kept.Length, Degree + 1, (row, col) => vandermonde[kept[row], col]);

    private static double TheilSenSlope(DataTwoFact[] points)
    {
        var slopes = new List<double>();
        for (var i = 0; i < points.Length; i++)
        for (var j = i + 1; j < points.Length; j++)
            if (points[j].Y != points[i].Y)
                slopes.Add((points[j].X1 - points[i].X1) / (points[j].Y - points[i].Y));

        slopes.Sort();
        var middle = slopes.Count / 2;
        return slopes.Count % 2 == 1 ? slopes[middle] : (slopes[middle - 1] + slopes[middle]) / 2;
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

    private static double[]? ResidualsWithout(int[] removed, Matrix<double> vandermonde, Vector<double> codes, DataTwoFact[] points)
    {
        var kept = KeptIndices(removed, points.Length);
        if (kept.Select(i => points[i].Y).Distinct().Count() <= Degree) return null;

        var coefficients = MultipleRegression.QR(KeptRows(kept, vandermonde), Vector<double>.Build.Dense(kept.Length, row => codes[kept[row]]));
        return (codes - vandermonde * coefficients).ToArray();
    }

    private static Matrix<double> CenteredVandermonde(DataTwoFact[] points)
    {
        var mean = points.Average(p => p.Y);
        var scale = Math.Sqrt(points.Average(p => (p.Y - mean) * (p.Y - mean)));
        return Matrix<double>.Build.Dense(points.Length, Degree + 1,
            (row, col) => Math.Pow((points[row].Y - mean) / scale, col));
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

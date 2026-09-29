using MathNet.Numerics;
using Regression.Two_factor_regression;

namespace SolversTests;

/// <summary>
/// Isothermal series for the IsothermalSeriesOutlierDetector tests: the 15 real series
/// (sensors 235, 223, 224 — 5 series of 11 consecutive points each) and deterministic
/// synthetic series with unequally spaced pressures spanning vacuum and overpressure.
/// </summary>
internal static class OutlierTestSeries
{
    public const double DefaultRelativeTolerance = 5e-5;

    /// <summary>δ ≈ 50τ: 0.25% of the series' pressure range.</summary>
    public const double LargeRelativeError = 50 * DefaultRelativeTolerance;

    /// <summary>δ ≈ 10τ: 0.05% of the series' pressure range — the sensitivity limit.</summary>
    public const double SmallRelativeError = 10 * DefaultRelativeTolerance;

    public static readonly double[] RelativeErrors = { LargeRelativeError, SmallRelativeError };

    public static IEnumerable<(string Name, DataTwoFact[] Points)> Real()
    {
        foreach (var (sensor, data) in new[]
                 {
                     ("235", SeriesCountAnalysisData.FiveOriginal),
                     ("223", Sensor223And224Data.Data223),
                     ("224", Sensor223And224Data.Data224),
                 })
            for (var s = 0; s < 5; s++)
                yield return ($"{sensor}/series {s}", data.Skip(11 * s).Take(11).ToArray());
    }

    /// <summary>Indices of the vacuum points in every <see cref="Synthetic"/> series.</summary>
    public static readonly int[] VacuumIndices = { 0, 1, 2 };

    public static IEnumerable<(string Name, DataTwoFact[] Points)> Synthetic()
    {
        for (var seed = 1; seed <= 10; seed++)
            yield return ($"synthetic seed {seed}", SyntheticSeries(seed));
    }

    public static IEnumerable<(string Name, DataTwoFact[] Points)> All() => Real().Concat(Synthetic());

    /// <summary>
    /// 3 vacuum points (negative), 0, 7 overpressure points, sorted by pressure. Pressures are
    /// cumulative sums of random multiples of a fixed step (unequally spaced, as with the
    /// dead-weight set whose smallest weight is 50 g). X1 is the inverse of a cubic fit of the real
    /// series 235 / 15 °C plus deterministic noise σ = 0.0003 codes; X2 comes from the same real series.
    /// </summary>
    public static DataTwoFact[] SyntheticSeries(int seed)
    {
        const double step = 2.4531;
        const double x1NoiseSigma = 0.0003;
        var random = new Random(seed);
        var real = SeriesCountAnalysisData.FiveOriginal.Take(11).ToArray();
        var cubic = Fit.Polynomial(real.Select(p => p.X1).ToArray(), real.Select(p => p.Y).ToArray(), 3);

        var vacuum = new double[3];
        for (var i = 0; i < 3; i++) vacuum[i] = (i == 0 ? 0 : vacuum[i - 1]) - step * random.Next(1, 9);
        var overpressure = new double[7];
        for (var i = 0; i < 7; i++) overpressure[i] = (i == 0 ? 0 : overpressure[i - 1]) + step * random.Next(1, 12);
        var pressures = vacuum.Reverse().Append(0).Concat(overpressure).ToArray();

        return pressures
            .Select((y, i) => new DataTwoFact
            {
                X1 = InverseOf(cubic, y, real[0].X1) + x1NoiseSigma * Gaussian(random),
                X2 = real[i].X2,
                Y = y,
            })
            .ToArray();
    }

    /// <summary>Copy of the series with <paramref name="errors"/> (index → error in Y units) added to Y.</summary>
    public static DataTwoFact[] WithErrors(DataTwoFact[] series, IReadOnlyDictionary<int, double> errors)
        => series.Select((p, i) => errors.TryGetValue(i, out var e) ? p with { Y = p.Y + e } : p).ToArray();

    public static double Range(DataTwoFact[] series) => series.Max(p => p.Y) - series.Min(p => p.Y);

    /// <summary>Distinct random indices of the series, sorted, each with a random-sign error of size δ.</summary>
    public static Dictionary<int, double> RandomErrors(Random random, int seriesLength, int count, double delta)
        => Enumerable.Range(0, seriesLength).OrderBy(_ => random.Next()).Take(count).OrderBy(i => i)
            .ToDictionary(i => i, _ => random.Next(2) == 0 ? delta : -delta);

    private static double InverseOf(double[] polynomial, double y, double x0)
    {
        var x = x0;
        for (var iteration = 0; iteration < 50; iteration++)
        {
            var value = Polynomial.Evaluate(x, polynomial) - y;
            var slope = polynomial[1] + 2 * polynomial[2] * x + 3 * polynomial[3] * x * x;
            x -= value / slope;
        }
        return x;
    }

    private static double Gaussian(Random random)
        => Math.Sqrt(-2 * Math.Log(1 - random.NextDouble())) * Math.Cos(2 * Math.PI * random.NextDouble());
}

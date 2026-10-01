namespace Regression.OutlierDetection;

internal static class TemperatureSeriesSplitter
{
    public static IEnumerable<int[]> SplitIntoSeries(CalibrationPoint[] points, double seriesTemperatureGap)
    {
        var byTemperature = Enumerable.Range(0, points.Length).OrderBy(row => points[row].Temperature).ThenBy(row => row).ToArray();
        var current = new List<int>();
        foreach (var row in byTemperature)
        {
            if (current.Count > 0 && points[row].Temperature - points[current[^1]].Temperature > seriesTemperatureGap)
            {
                yield return current.OrderBy(r => r).ToArray();
                current = new List<int>();
            }
            current.Add(row);
        }
        if (current.Count > 0)
            yield return current.OrderBy(r => r).ToArray();
    }

    public static double NominalTemperature(CalibrationPoint[] points, int[] rows)
        => Math.Round(Median(rows.Select(row => points[row].Temperature)), MidpointRounding.AwayFromZero);

    private static double Median(IEnumerable<double> values)
    {
        var sorted = values.OrderBy(value => value).ToArray();
        var middle = sorted.Length / 2;
        return sorted.Length % 2 == 1 ? sorted[middle] : (sorted[middle - 1] + sorted[middle]) / 2;
    }
}

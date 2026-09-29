using Regression.Two_factor_regression;

namespace Regression.OutlierDetection;

public class CalibrationDatasetChecker
{
    private readonly IsothermalSeriesOutlierDetector _detector;
    private readonly double _seriesTemperatureGap;

    public CalibrationDatasetChecker(double accuracyClassPercent = 0.01, double seriesTemperatureGap = 2.0)
    {
        if (!(accuracyClassPercent > 0))
            throw new ArgumentOutOfRangeException(nameof(accuracyClassPercent), accuracyClassPercent, "The accuracy class must be positive.");
        if (!(seriesTemperatureGap > 0))
            throw new ArgumentOutOfRangeException(nameof(seriesTemperatureGap), seriesTemperatureGap, "The temperature gap between series must be positive.");

        _detector = new IsothermalSeriesOutlierDetector(accuracyClassPercent);
        _seriesTemperatureGap = seriesTemperatureGap;
    }

    public DatasetCheckResult Check(IEnumerable<CalibrationPoint> dataset)
    {
        if (dataset is null) throw new ArgumentNullException(nameof(dataset));
        var points = dataset.ToArray();

        var series = SplitIntoSeries(points)
            .Select(rows => CheckSeries(points, rows))
            .OrderBy(check => check.NominalTemperature)
            .ToList();
        return new DatasetCheckResult(series);
    }

    private IEnumerable<int[]> SplitIntoSeries(CalibrationPoint[] points)
    {
        var byTemperature = Enumerable.Range(0, points.Length).OrderBy(row => points[row].Temperature).ThenBy(row => row).ToArray();
        var current = new List<int>();
        foreach (var row in byTemperature)
        {
            if (current.Count > 0 && points[row].Temperature - points[current[^1]].Temperature > _seriesTemperatureGap)
            {
                yield return current.OrderBy(r => r).ToArray();
                current = new List<int>();
            }
            current.Add(row);
        }
        if (current.Count > 0)
            yield return current.OrderBy(r => r).ToArray();
    }

    private SeriesCheck CheckSeries(CalibrationPoint[] points, int[] rows)
    {
        var nominal = Math.Round(Median(rows.Select(row => points[row].Temperature)), MidpointRounding.AwayFromZero);
        var series = rows
            .Select(row => new DataTwoFact { X1 = points[row].PressureCode, X2 = points[row].TemperatureCode, Y = points[row].Pressure })
            .ToArray();

        try
        {
            var suspicious = _detector.GetSuspiciousPoints(series)
                .Select(point => new DatasetSuspiciousPoint(rows[point.Index], nominal, point))
                .ToList();
            return new CheckedSeries(nominal, rows, suspicious);
        }
        catch (SeriesNotResolvableException exception)
        {
            return new UnresolvableSeries(nominal, rows, exception.MaxOutliers, exception.Tolerance);
        }
        catch (ArgumentException exception)
        {
            return new SkippedSeries(nominal, rows, exception.Message);
        }
    }

    private static double Median(IEnumerable<double> values)
    {
        var sorted = values.OrderBy(value => value).ToArray();
        var middle = sorted.Length / 2;
        return sorted.Length % 2 == 1 ? sorted[middle] : (sorted[middle - 1] + sorted[middle]) / 2;
    }
}

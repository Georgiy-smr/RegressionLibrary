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

        var series = TemperatureSeriesSplitter.SplitIntoSeries(points, _seriesTemperatureGap)
            .Select(rows => CheckSeries(points, rows))
            .OrderBy(check => check.NominalTemperature)
            .ToList();
        return new DatasetCheckResult(series);
    }

    private SeriesCheck CheckSeries(CalibrationPoint[] points, int[] rows)
    {
        var nominal = TemperatureSeriesSplitter.NominalTemperature(points, rows);
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
}

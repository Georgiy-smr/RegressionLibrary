namespace Regression.OutlierDetection;

public sealed record CalibrationPoint(double PressureCode, double TemperatureCode, double Pressure, double Temperature);

public sealed record DatasetSuspiciousPoint(int Row, double NominalTemperature, SuspiciousPoint Detail);

public abstract record SeriesCheck(double NominalTemperature, IReadOnlyList<int> Rows);

public sealed record CheckedSeries(double NominalTemperature, IReadOnlyList<int> Rows, IReadOnlyList<DatasetSuspiciousPoint> SuspiciousPoints)
    : SeriesCheck(NominalTemperature, Rows);

public sealed record UnresolvableSeries(double NominalTemperature, IReadOnlyList<int> Rows, int MaxOutliers, double Tolerance)
    : SeriesCheck(NominalTemperature, Rows);

public sealed record SkippedSeries(double NominalTemperature, IReadOnlyList<int> Rows, string Reason)
    : SeriesCheck(NominalTemperature, Rows);

public sealed record DatasetCheckResult(IReadOnlyList<SeriesCheck> Series)
{
    public IReadOnlyList<DatasetSuspiciousPoint> SuspiciousPoints { get; } = Series
        .OfType<CheckedSeries>()
        .SelectMany(series => series.SuspiciousPoints)
        .OrderBy(point => point.Row)
        .ToList();

    public bool IsClean { get; } = Series.All(series => series is CheckedSeries { SuspiciousPoints.Count: 0 });
}

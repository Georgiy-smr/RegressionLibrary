using Regression.OutlierDetection;

namespace Regression.SectorCompensation;

public sealed record SeriesError(double Temperature, int Count, double Max, double Mean);

public sealed class SensorModelError
{
    private readonly ISensorModel _model;
    private readonly CalibrationPoint[] _points;

    public SensorModelError(ISensorModel model, IEnumerable<CalibrationPoint> points)
    {
        if (points is null) throw new ArgumentNullException(nameof(points));

        _model = model ?? throw new ArgumentNullException(nameof(model));
        _points = points.ToArray();
        if (_points.Length == 0)
            throw new ArgumentException("At least one point is required.", nameof(points));
    }

    public double GetCurrent(CalibrationPoint point)
        => Math.Abs(point.Pressure - _model.GetPressure(point.PressureCode, point.TemperatureCode));

    public double GetMax() => _points.Max(GetCurrent);

    public double GetMean() => _points.Average(GetCurrent);

    public IReadOnlyList<SeriesError> GetBySeries()
        => _points
            .GroupBy(point => point.Temperature)
            .OrderBy(series => series.Key)
            .Select(series => new SeriesError(series.Key, series.Count(), series.Max(GetCurrent), series.Average(GetCurrent)))
            .ToList();
}

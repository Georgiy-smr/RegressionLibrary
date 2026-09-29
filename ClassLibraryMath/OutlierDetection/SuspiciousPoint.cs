using Regression.Two_factor_regression;

namespace Regression.OutlierDetection;

/// <summary>
/// A point of an isothermal series that <see cref="IsothermalSeriesOutlierDetector"/> flags as
/// suspicious. <see cref="Index"/> is the point's position in the input series.
/// </summary>
public abstract record SuspiciousPoint(int Index, DataTwoFact Point);

/// <summary>
/// A confidently identified outlier: the minimal outlier set is unique.
/// <see cref="CodeError"/> = Point.X1 − g(Point.Y), where g is the characteristic fitted to the
/// clean points: how far the recorded pressure code is from the code the sensor gives at the
/// target pressure Point.Y, in X1 units.
/// </summary>
public sealed record Outlier(int Index, DataTwoFact Point, double CodeError) : SuspiciousPoint(Index, Point);

/// <summary>
/// The point belongs to one of several equally good minimal outlier sets, so the detector
/// cannot tell which of them is wrong. The point has to be re-checked.
/// </summary>
public sealed record AmbiguousPoint(int Index, DataTwoFact Point) : SuspiciousPoint(Index, Point);

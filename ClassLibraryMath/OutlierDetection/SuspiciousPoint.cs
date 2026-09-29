using Regression.Two_factor_regression;

namespace Regression.OutlierDetection;

/// <summary>
/// A point of an isothermal series that <see cref="IsothermalSeriesOutlierDetector"/> flags as
/// suspicious. <see cref="Index"/> is the point's position in the input series.
/// </summary>
public abstract record SuspiciousPoint(int Index, DataTwoFact Point);

/// <summary>
/// A confidently identified outlier: the minimal outlier set is unique.
/// <see cref="Residual"/> = Point.Y − f(Point.X1), where f is the curve fitted to the clean
/// points, in Y units.
/// <see cref="CodeError"/> = Point.X1 − f⁻¹(Point.Y), in X1 units: how far the recorded pressure
/// code is from the code the clean characteristic gives at the reference pressure Point.Y.
/// </summary>
public sealed record Outlier(int Index, DataTwoFact Point, double Residual, double CodeError) : SuspiciousPoint(Index, Point);

/// <summary>
/// The point belongs to one of several equally good minimal outlier sets, so the detector
/// cannot tell which of them is wrong. The point has to be re-checked.
/// </summary>
public sealed record AmbiguousPoint(int Index, DataTwoFact Point) : SuspiciousPoint(Index, Point);

using Regression.Two_factor_regression;

namespace Regression.OutlierDetection;

public abstract record SuspiciousPoint(int Index, DataTwoFact Point);

public sealed record Outlier(int Index, DataTwoFact Point, double CodeError) : SuspiciousPoint(Index, Point);

public sealed record AmbiguousPoint(int Index, DataTwoFact Point) : SuspiciousPoint(Index, Point);

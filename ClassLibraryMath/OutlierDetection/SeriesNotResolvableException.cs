namespace Regression.OutlierDetection;

public class SeriesNotResolvableException : Exception
{
    public SeriesNotResolvableException(int maxOutliers, double tolerance)
        : base($"No set of at most {maxOutliers} points can be removed so that the rest of the series " +
               $"lies on a smooth curve within tolerance {tolerance:G6}. Re-measure the series or review the threshold.")
    {
        MaxOutliers = maxOutliers;
        Tolerance = tolerance;
    }

    public int MaxOutliers { get; }

    public double Tolerance { get; }
}

namespace Regression.OutlierDetection;

/// <summary>
/// The series cannot be resolved: there is no set of at most <see cref="MaxOutliers"/> points
/// whose removal leaves the rest on a smooth curve with noise ≤ <see cref="Tolerance"/>.
/// The series has to be re-measured, or the threshold reviewed.
/// </summary>
public class SeriesNotResolvableException : Exception
{
    public SeriesNotResolvableException(int maxOutliers, double tolerance)
        : base($"No set of at most {maxOutliers} points can be removed so that the rest of the series " +
               $"lies on a smooth curve within tolerance {tolerance:G6}. Re-measure the series or review the threshold.")
    {
        MaxOutliers = maxOutliers;
        Tolerance = tolerance;
    }

    /// <summary>The largest outlier set that was searched.</summary>
    public int MaxOutliers { get; }

    /// <summary>The noise threshold τ that was used, in X1 (pressure code) units.</summary>
    public double Tolerance { get; }
}

using Regression.OutlierDetection;

namespace SolversTests.Synthetic;

public static class SafeResult
{
    public static void AssertSafe(IReadOnlyList<SuspiciousPoint> result, params int[] injected)
    {
        Assert.NotEmpty(result);
        var indices = result.Select(point => point.Index).ToArray();

        if (result.All(point => point is Outlier))
            Assert.Equal(injected.OrderBy(index => index), indices);
        else
        {
            Assert.All(result, point => Assert.IsType<AmbiguousPoint>(point));
            Assert.Subset(indices.ToHashSet(), injected.ToHashSet());
        }
    }
}

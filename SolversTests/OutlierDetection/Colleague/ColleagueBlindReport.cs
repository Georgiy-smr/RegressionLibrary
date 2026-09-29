using System.Globalization;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.LinearRegression;
using Regression.OutlierDetection;
using Regression.Two_factor_regression;
using Xunit.Abstractions;

namespace SolversTests.Colleague;

public class ColleagueBlindReport
{
    private const double RawCodesPerScaledCode = 100000;
    private const double DefaultAccuracyClassPercent = 0.01;

    private readonly ITestOutputHelper _output;

    public ColleagueBlindReport(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Sample1() => Report(ColleagueSamples.Sample1);

    [Fact]
    public void Sample2() => Report(ColleagueSamples.Sample2);

    [Fact]
    public void Sample3() => Report(ColleagueSamples.Sample3);

    [Fact]
    public void Sample4() => Report(ColleagueSamples.Sample4);

    private void Report(DataTwoFact[] sample)
    {
        var slope = TheilSenSlope(sample);
        var tolerance = DefaultAccuracyClassPercent / 100 / 2 * (sample.Max(p => p.Y) - sample.Min(p => p.Y)) * Math.Abs(slope);
        var leaveOneOut = Enumerable.Range(0, sample.Length).Select(i => LeaveOneOutResidual(sample, i)).ToArray();
        var leverages = Leverages(sample);

        _output.WriteLine($"Result: {DetectorResult(sample)}");
        _output.WriteLine(Invariant($"Tau: {tolerance * RawCodesPerScaledCode:F1} raw codes; median |LOO residual|: {Median(leaveOneOut.Select(Math.Abs)) * RawCodesPerScaledCode:F1} raw codes; Theil-Sen slope: {slope * RawCodesPerScaledCode:F2} raw codes per pressure unit"));
        _output.WriteLine("| # (1-based) | P | LOO residual, raw codes | LOO residual, pressure | leverage |");
        _output.WriteLine("|---|---|---|---|---|");
        for (var i = 0; i < sample.Length; i++)
            _output.WriteLine(Invariant($"| {i + 1} | {sample[i].Y} | {leaveOneOut[i] * RawCodesPerScaledCode:F1} | {leaveOneOut[i] / slope:F4} | {leverages[i]:F2} |"));
    }

    private static string DetectorResult(DataTwoFact[] sample)
    {
        try
        {
            var result = new IsothermalSeriesOutlierDetector().GetSuspiciousPoints(sample);
            if (result.Count == 0)
                return "empty";
            return string.Join("; ", result.Select(point => point switch
            {
                Outlier outlier => Invariant($"Outlier index {outlier.Index} (#{outlier.Index + 1}), CodeError {outlier.CodeError * RawCodesPerScaledCode:F1} raw codes"),
                _ => $"AmbiguousPoint index {point.Index} (#{point.Index + 1})",
            }));
        }
        catch (SeriesNotResolvableException exception)
        {
            return Invariant($"SeriesNotResolvableException: MaxOutliers {exception.MaxOutliers}, Tolerance {exception.Tolerance * RawCodesPerScaledCode:F1} raw codes");
        }
    }

    private static double LeaveOneOutResidual(DataTwoFact[] sample, int left)
    {
        var design = CenteredVandermonde(sample);
        var kept = Enumerable.Range(0, sample.Length).Where(i => i != left).ToArray();
        var keptDesign = Matrix<double>.Build.Dense(kept.Length, 3, (row, col) => design[kept[row], col]);
        var keptCodes = Vector<double>.Build.Dense(kept.Length, row => sample[kept[row]].X1);
        var coefficients = MultipleRegression.QR(keptDesign, keptCodes);
        return sample[left].X1 - design.Row(left).DotProduct(coefficients);
    }

    private static double[] Leverages(DataTwoFact[] sample)
    {
        var q = CenteredVandermonde(sample).QR(QRMethod.Thin).Q;
        return Enumerable.Range(0, sample.Length).Select(row => q.Row(row).DotProduct(q.Row(row))).ToArray();
    }

    private static Matrix<double> CenteredVandermonde(DataTwoFact[] sample)
    {
        var mean = sample.Average(p => p.Y);
        var scale = Math.Sqrt(sample.Average(p => (p.Y - mean) * (p.Y - mean)));
        return Matrix<double>.Build.Dense(sample.Length, 3, (row, col) => Math.Pow((sample[row].Y - mean) / scale, col));
    }

    private static double TheilSenSlope(DataTwoFact[] sample)
    {
        var slopes = new List<double>();
        for (var i = 0; i < sample.Length; i++)
        for (var j = i + 1; j < sample.Length; j++)
            if (sample[j].Y != sample[i].Y)
                slopes.Add((sample[j].X1 - sample[i].X1) / (sample[j].Y - sample[i].Y));
        return Median(slopes);
    }

    private static double Median(IEnumerable<double> values)
    {
        var sorted = values.OrderBy(v => v).ToArray();
        var middle = sorted.Length / 2;
        return sorted.Length % 2 == 1 ? sorted[middle] : (sorted[middle - 1] + sorted[middle]) / 2;
    }

    private static string Invariant(FormattableString text) => text.ToString(CultureInfo.InvariantCulture);
}

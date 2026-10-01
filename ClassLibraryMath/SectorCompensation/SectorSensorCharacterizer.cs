using MathNet.Numerics.LinearAlgebra;
using Regression.OutlierDetection;

namespace Regression.SectorCompensation;

public sealed class SectorSensorCharacterizer : ISensorCharacterizer
{
    private static readonly (int Pressure, int Temperature)[] LowerTriangle = { (0, 0), (1, 0), (2, 0), (0, 1), (1, 1), (0, 2) };
    private static readonly (int Pressure, int Temperature)[] UpperTriangle = { (2, 2), (1, 2), (0, 2), (2, 1), (1, 1), (2, 0) };

    private readonly PolynomialSensorCharacterizer _roughCharacterizer;
    private readonly int[]? _nodePressureIndices;
    private readonly double _seriesTemperatureGap;

    public SectorSensorCharacterizer(PolynomialSensorCharacterizer roughCharacterizer, IEnumerable<int>? nodePressureIndices = null, double seriesTemperatureGap = 2.0)
    {
        if (!(seriesTemperatureGap > 0))
            throw new ArgumentOutOfRangeException(nameof(seriesTemperatureGap), seriesTemperatureGap, "The temperature gap between series must be positive.");

        _roughCharacterizer = roughCharacterizer ?? throw new ArgumentNullException(nameof(roughCharacterizer));
        _nodePressureIndices = nodePressureIndices?.OrderBy(index => index).ToArray();
        _seriesTemperatureGap = seriesTemperatureGap;
    }

    public SectorSensor Characterize(IEnumerable<CalibrationPoint> points)
    {
        if (points is null) throw new ArgumentNullException(nameof(points));
        var calibration = points.ToArray();

        return Characterize(calibration, _roughCharacterizer.Characterize(calibration));
    }

    public SectorSensor Characterize(IEnumerable<CalibrationPoint> points, PolynomialSensor roughSensor)
    {
        if (points is null) throw new ArgumentNullException(nameof(points));
        if (roughSensor is null) throw new ArgumentNullException(nameof(roughSensor));
        var calibration = points.ToArray();

        var series = SplitIntoSeries(calibration);
        var nodes = BuildNodes(series, roughSensor);

        var sectors = new List<Sector>();
        for (var temperature = 0; temperature + 2 < nodes.Length; temperature += 2)
        for (var pressure = 0; pressure + 2 < nodes[temperature].Length; pressure += 2)
        {
            sectors.Add(BuildSector(sectors.Count + 1, nodes, pressure, temperature, LowerTriangle));
            sectors.Add(BuildSector(sectors.Count + 1, nodes, pressure, temperature, UpperTriangle));
        }
        return new SectorSensor(roughSensor, sectors);
    }

    ISensorModel ISensorCharacterizer.Characterize(IEnumerable<CalibrationPoint> points) => Characterize(points);

    private CalibrationPoint[][] SplitIntoSeries(CalibrationPoint[] points)
    {
        var series = TemperatureSeriesSplitter.SplitIntoSeries(points, _seriesTemperatureGap)
            .OrderBy(rows => TemperatureSeriesSplitter.NominalTemperature(points, rows))
            .Select(rows => rows.Select(row => points[row]).OrderBy(point => point.Pressure).ToArray())
            .ToArray();

        if (series.Length > 0 && series.Any(s => !s.Select(point => point.Pressure).SequenceEqual(series[0].Select(point => point.Pressure))))
            throw new ArgumentException("All temperature series must have the same set of pressures.", nameof(points));
        return series;
    }

    private Node[][] BuildNodes(CalibrationPoint[][] series, PolynomialSensor roughSensor)
    {
        ValidateNodeCount(series.Length, "temperature");

        var nodePressureIndices = _nodePressureIndices ?? Enumerable.Range(0, series[0].Length).ToArray();
        ValidateNodeCount(nodePressureIndices.Length, "pressure");
        if (nodePressureIndices.Distinct().Count() != nodePressureIndices.Length)
            throw new ArgumentException("The node pressure indices must be distinct.");
        if (nodePressureIndices[0] < 0 || nodePressureIndices[^1] >= series[0].Length)
            throw new ArgumentException($"The node pressure indices must be between 0 and {series[0].Length - 1}.");

        return series
            .Select(s => nodePressureIndices
                .Select(index => s[index])
                .Select(point => new Node(point.Pressure, roughSensor.GetTemperature(point.PressureCode, point.TemperatureCode), point.PressureCode))
                .ToArray())
            .ToArray();
    }

    private static void ValidateNodeCount(int count, string axis)
    {
        if (count < 3 || count % 2 == 0)
            throw new ArgumentException($"The number of {axis} nodes must be odd and at least 3, but got {count}.");
    }

    private static Sector BuildSector(int number, Node[][] nodes, int pressure, int temperature, (int Pressure, int Temperature)[] triangle)
    {
        var sectorNodes = triangle.Select(offset => nodes[temperature + offset.Temperature][pressure + offset.Pressure]).ToArray();
        var anchor = sectorNodes[0];

        var matrix = Matrix<double>.Build.DenseOfRows(sectorNodes.Select(node =>
        {
            var dP = node.Pressure - anchor.Pressure;
            var dT = node.Temperature - anchor.Temperature;
            return new[] { 1, dP, dT, dP * dP, dT * dT, dP * dT };
        }));
        var codes = Vector<double>.Build.DenseOfEnumerable(sectorNodes.Select(node => node.PressureCode));
        var coefficients = matrix.Solve(codes);

        return new Sector(number, Vertex(sectorNodes[0]), Vertex(sectorNodes[2]), Vertex(sectorNodes[5]), coefficients);
    }

    private static SectorNode Vertex(Node node) => new(node.Pressure, node.Temperature);

    private sealed record Node(double Pressure, double Temperature, double PressureCode);
}

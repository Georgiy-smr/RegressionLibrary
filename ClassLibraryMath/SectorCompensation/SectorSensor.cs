namespace Regression.SectorCompensation;

public sealed class SectorSensor : ISensorModel
{
    private const double LinearEquationThreshold = 1e-12;

    private readonly Sector[] _sectors;
    private readonly double _pressureRange;
    private readonly double _temperatureRange;

    public SectorSensor(PolynomialSensor roughSensor, IEnumerable<Sector> sectors)
    {
        if (sectors is null) throw new ArgumentNullException(nameof(sectors));

        RoughSensor = roughSensor ?? throw new ArgumentNullException(nameof(roughSensor));
        _sectors = sectors.ToArray();
        if (_sectors.Length == 0)
            throw new ArgumentException("The sensor must have at least one sector.", nameof(sectors));

        var vertices = _sectors.SelectMany(Vertices).ToArray();
        _pressureRange = vertices.Max(vertex => vertex.Pressure) - vertices.Min(vertex => vertex.Pressure);
        _temperatureRange = vertices.Max(vertex => vertex.Temperature) - vertices.Min(vertex => vertex.Temperature);
    }

    public PolynomialSensor RoughSensor { get; }

    public IReadOnlyList<Sector> Sectors => _sectors;

    public IReadOnlyList<double> Values => RoughSensor.Values.Concat(_sectors.SelectMany(sector => sector.Values)).ToArray();

    public int CoefficientCount => RoughSensor.CoefficientCount + 6 * _sectors.Length;

    public double GetPressure(double pressureCode, double temperatureCode) => Trace(pressureCode, temperatureCode).Pressure;

    public SectorTrace Trace(double pressureCode, double temperatureCode)
    {
        var roughPressure = RoughSensor.GetPressure(pressureCode, temperatureCode);
        var temperature = RoughSensor.GetTemperature(pressureCode, temperatureCode);

        var sector = FindContaining(roughPressure, temperature) ?? FindNearest(roughPressure, temperature);
        var pressure = SolvePressure(sector, pressureCode, temperature, roughPressure);

        var refined = FindContaining(pressure, temperature);
        if (refined is not null && refined.Number != sector.Number)
        {
            sector = refined;
            pressure = SolvePressure(sector, pressureCode, temperature, pressure);
        }

        return new SectorTrace(roughPressure, temperature, sector.Number, pressure);
    }

    private Sector? FindContaining(double pressure, double temperature)
        => _sectors.FirstOrDefault(sector => Contains(sector, pressure, temperature));

    private static bool Contains(Sector sector, double pressure, double temperature)
    {
        var s1 = Side(sector.Vertex1, sector.Vertex2, pressure, temperature);
        var s2 = Side(sector.Vertex2, sector.Vertex3, pressure, temperature);
        var s3 = Side(sector.Vertex3, sector.Vertex1, pressure, temperature);
        return (s1 >= 0 && s2 >= 0 && s3 >= 0) || (s1 <= 0 && s2 <= 0 && s3 <= 0);
    }

    private static double Side(SectorNode from, SectorNode to, double pressure, double temperature)
        => (to.Pressure - from.Pressure) * (temperature - from.Temperature) - (to.Temperature - from.Temperature) * (pressure - from.Pressure);

    private Sector FindNearest(double pressure, double temperature)
    {
        var nearest = _sectors[0];
        var nearestDistance = double.PositiveInfinity;
        foreach (var sector in _sectors)
        {
            var distance = DistanceToCentre(sector, pressure, temperature);
            if (distance < nearestDistance)
            {
                nearest = sector;
                nearestDistance = distance;
            }
        }
        return nearest;
    }

    private double DistanceToCentre(Sector sector, double pressure, double temperature)
    {
        var centrePressure = (sector.Vertex1.Pressure + sector.Vertex2.Pressure + sector.Vertex3.Pressure) / 3;
        var centreTemperature = (sector.Vertex1.Temperature + sector.Vertex2.Temperature + sector.Vertex3.Temperature) / 3;
        var dP = (pressure - centrePressure) / _pressureRange;
        var dT = (temperature - centreTemperature) / _temperatureRange;
        return Math.Sqrt(dP * dP + dT * dT);
    }

    private static double SolvePressure(Sector sector, double pressureCode, double temperature, double nearPressure)
    {
        var dT = temperature - sector.Anchor.Temperature;
        var a = sector.C3;
        var b = sector.C1 + sector.C5 * dT;
        var c = sector.C0 + sector.C2 * dT + sector.C4 * dT * dT - pressureCode;

        if (Math.Abs(a) <= LinearEquationThreshold * Math.Abs(b))
            return sector.Anchor.Pressure - c / b;

        var discriminant = b * b - 4 * a * c;
        if (discriminant < 0)
            throw new InvalidOperationException($"Sector {sector.Number} has no pressure for pressure code {pressureCode} at temperature {temperature}: the discriminant is negative.");

        var q = -(b + Math.Sign(b) * Math.Sqrt(discriminant)) / 2;
        var root1 = q / a;
        var root2 = c / q;
        var nearDp = nearPressure - sector.Anchor.Pressure;
        var dP = Math.Abs(root1 - nearDp) <= Math.Abs(root2 - nearDp) ? root1 : root2;
        return sector.Anchor.Pressure + dP;
    }

    private static IEnumerable<SectorNode> Vertices(Sector sector)
    {
        yield return sector.Vertex1;
        yield return sector.Vertex2;
        yield return sector.Vertex3;
    }
}

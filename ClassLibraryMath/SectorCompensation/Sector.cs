namespace Regression.SectorCompensation;

public sealed record SectorNode(double Pressure, double Temperature);

public sealed class Sector
{
    private const int CoefficientCount = 6;

    public Sector(int number, SectorNode vertex1, SectorNode vertex2, SectorNode vertex3, IEnumerable<double> coefficients)
    {
        if (coefficients is null) throw new ArgumentNullException(nameof(coefficients));

        Number = number;
        Vertex1 = vertex1 ?? throw new ArgumentNullException(nameof(vertex1));
        Vertex2 = vertex2 ?? throw new ArgumentNullException(nameof(vertex2));
        Vertex3 = vertex3 ?? throw new ArgumentNullException(nameof(vertex3));
        Coefficients = coefficients.ToArray();

        if (Coefficients.Count != CoefficientCount)
            throw new ArgumentException($"A sector must have {CoefficientCount} coefficients, but got {Coefficients.Count}.", nameof(coefficients));
    }

    public int Number { get; }

    public SectorNode Vertex1 { get; }

    public SectorNode Vertex2 { get; }

    public SectorNode Vertex3 { get; }

    public IReadOnlyList<double> Coefficients { get; }

    public IReadOnlyList<double> Values => new[]
        {
            Vertex1.Pressure, Vertex1.Temperature,
            Vertex2.Pressure, Vertex2.Temperature,
            Vertex3.Pressure, Vertex3.Temperature,
        }
        .Concat(Coefficients)
        .ToArray();
}

public sealed record SectorTrace(double RoughPressure, double RoughTemperature, int SectorNumber, double Pressure);

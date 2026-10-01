namespace Regression.SectorCompensation;

public sealed record SectorNode(double Pressure, double Temperature);

public sealed record Sector(
    int Number,
    SectorNode Anchor,
    SectorNode Vertex1,
    SectorNode Vertex2,
    SectorNode Vertex3,
    double C0,
    double C1,
    double C2,
    double C3,
    double C4,
    double C5)
{
    public IReadOnlyList<double> Values => new[]
    {
        Vertex1.Pressure, Vertex1.Temperature,
        Vertex2.Pressure, Vertex2.Temperature,
        Vertex3.Pressure, Vertex3.Temperature,
        C0, C1, C2, C3, C4, C5,
    };
}

public sealed record SectorTrace(double RoughPressure, double RoughTemperature, int SectorNumber, double Pressure);

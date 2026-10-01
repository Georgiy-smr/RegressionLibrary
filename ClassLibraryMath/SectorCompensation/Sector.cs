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
    double C5);

public sealed record SectorTrace(double RoughPressure, double RoughTemperature, int SectorNumber, double Pressure);

namespace Regression.SectorCompensation;

public interface ISensorModel
{
    double GetPressure(double pressureCode, double temperatureCode);
}

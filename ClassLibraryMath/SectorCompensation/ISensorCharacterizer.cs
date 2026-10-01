using Regression.OutlierDetection;

namespace Regression.SectorCompensation;

public interface ISensorCharacterizer
{
    ISensorModel Characterize(IEnumerable<CalibrationPoint> points);
}

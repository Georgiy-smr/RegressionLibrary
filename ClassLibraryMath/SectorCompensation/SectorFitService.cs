using Regression.OutlierDetection;

namespace Regression.SectorCompensation;

public sealed class SectorFitService
{
    private readonly SectorSensorCharacterizer _characterizer;

    public SectorFitService(SectorSensorCharacterizer characterizer)
    {
        _characterizer = characterizer ?? throw new ArgumentNullException(nameof(characterizer));
    }

    public IEnumerable<double> GetValues(IEnumerable<CalibrationPoint> points) => _characterizer.Characterize(points).Values;
}

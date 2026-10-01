using Regression.OutlierDetection;
using Regression.Two_factor_regression;
using Regression.Two_factor_regression.Implements;
using Regression.Two_factor_regression.Interfaces;

namespace Regression.SectorCompensation;

public sealed class PolynomialSensorCharacterizer : ISensorCharacterizer
{
    private readonly PolynomialLeastSquaresSolver _pressureSolver;
    private readonly PolynomialLeastSquaresSolver _temperatureSolver;

    public PolynomialSensorCharacterizer()
        : this(new ThirdOrderBasisExponents(), new SecondOrderBasisExponents())
    {
    }

    public PolynomialSensorCharacterizer(IBasisExponents pressureBasis, IBasisExponents temperatureBasis)
    {
        if (pressureBasis is null) throw new ArgumentNullException(nameof(pressureBasis));
        if (temperatureBasis is null) throw new ArgumentNullException(nameof(temperatureBasis));

        _pressureSolver = new PolynomialLeastSquaresSolver(pressureBasis);
        _temperatureSolver = new PolynomialLeastSquaresSolver(temperatureBasis);
    }

    public PolynomialSensor Characterize(IEnumerable<CalibrationPoint> points)
    {
        if (points is null) throw new ArgumentNullException(nameof(points));
        var calibration = points.ToArray();

        var pressure = calibration.Select(point => new DataTwoFact { X1 = point.PressureCode, X2 = point.TemperatureCode, Y = point.Pressure });
        var temperature = calibration.Select(point => new DataTwoFact { X1 = point.PressureCode, X2 = point.TemperatureCode, Y = point.Temperature });

        return new PolynomialSensor(_pressureSolver.GetValues(pressure), _temperatureSolver.GetValues(temperature));
    }

    ISensorModel ISensorCharacterizer.Characterize(IEnumerable<CalibrationPoint> points) => Characterize(points);
}

using Regression.SectorCompensation;

namespace SolversTests.SensorComparison;

public class GridValidationTests
{
    [Fact]
    public void EvenNumberOfPressureNodes()
    {
        var characterizer = new SectorSensorCharacterizer(new PolynomialSensorCharacterizer(), new[] { 0, 2, 5, 10 });

        Assert.Throws<ArgumentException>(() => characterizer.Characterize(Sensor00249979Dataset.Points));
    }

    [Fact]
    public void FewerThanThreePressureNodes()
    {
        var characterizer = new SectorSensorCharacterizer(new PolynomialSensorCharacterizer(), new[] { 5 });

        Assert.Throws<ArgumentException>(() => characterizer.Characterize(Sensor00249979Dataset.Points));
    }

    [Fact]
    public void EvenNumberOfTemperatureNodes()
    {
        var characterizer = new SectorSensorCharacterizer(new PolynomialSensorCharacterizer());

        Assert.Throws<ArgumentException>(() => characterizer.Characterize(Sensor00249979Dataset.Points.Take(44)));
    }

    [Fact]
    public void DifferentPressureSetsInSeries()
    {
        var characterizer = new SectorSensorCharacterizer(new PolynomialSensorCharacterizer());
        var points = Sensor00249979Dataset.Points.ToArray();
        points[3] = points[3] with { Pressure = 91 };

        Assert.Throws<ArgumentException>(() => characterizer.Characterize(points));
    }
}

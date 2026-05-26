namespace SEP4.Tests;

public class SensorValidationTests
{
    [Fact]
    public void TemperatureTooLow_ShouldFail()
    {
        double temperature = -300;

        bool valid = temperature >= -50 && temperature <= 100;

        Assert.False(valid);
    }

    [Fact]
    public void TemperatureWithinRange_ShouldPass()
    {
        double temperature = 22.5;

        bool valid = temperature >= -50 && temperature <= 100;

        Assert.True(valid);
    }
}
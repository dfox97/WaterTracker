using FluentAssertions;
using WaterTracker.Features.WaterIntake; 
namespace WaterTracker.Tests.Features.WaterIntake;

public class WaterIntakeValidatorTests
{
    private readonly WaterIntakeValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(10001)]
    public void AmountMl_InvalidValues_FailValidation(int amount)
    {
        var request = new CreateIntakeRequest(amount, DateTimeOffset.UtcNow, null);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }


    [Theory]
    [InlineData(1)]
    [InlineData(250)]
    [InlineData(10000)]
    public void AmountMl_ValidValues_PassValidation(int amount)
    {
        var request = new CreateIntakeRequest(amount, DateTimeOffset.UtcNow, null);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }


    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-1440)]
    public void RecordedAt_ValidDates_PassValidation(int minuteOffset)
    {
        var request = new CreateIntakeRequest(
            250,
            DateTimeOffset.UtcNow.AddMinutes(minuteOffset),
            null);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    public void RecordedAt_FutureDates_FailValidation(int minuteOffset)
    {
        var request = new CreateIntakeRequest(
            250,
            DateTimeOffset.UtcNow.AddMinutes(minuteOffset),
            null);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(500, true)]
    [InlineData(501, false)]
    public void Notes_LengthValidation_Works(int length, bool expectedValid)
    {
        var notes = new string('a', length);

        var request = new CreateIntakeRequest(250, DateTimeOffset.UtcNow, notes);

        var result = _validator.Validate(request);

        result.IsValid.Should().Be(expectedValid);
    }
}

public class UpdateIntakeValidatorTests
{
    private readonly UpdateIntakeValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(10001)]
    public void AmountMl_InvalidValues_FailValidation(int amount)
    {
        var request = new UpdateIntakeRequest(amount, DateTimeOffset.UtcNow, null);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(250)]
    [InlineData(10000)]
    public void AmountMl_ValidValues_PassValidation(int amount)
    {
        var request = new UpdateIntakeRequest(amount, DateTimeOffset.UtcNow, null);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    public void RecordedAt_FutureDates_FailValidation(int minuteOffset)
    {
        var request = new UpdateIntakeRequest(
            250,
            DateTimeOffset.UtcNow.AddMinutes(minuteOffset),
            null);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(500, true)]
    [InlineData(501, false)]
    public void Notes_LengthValidation_Works(int length, bool expectedValid)
    {
        var notes = new string('a', length);

        var request = new UpdateIntakeRequest(250, DateTimeOffset.UtcNow, notes);

        var result = _validator.Validate(request);

        result.IsValid.Should().Be(expectedValid);
    }
}
 
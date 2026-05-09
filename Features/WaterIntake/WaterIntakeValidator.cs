using FluentValidation;

namespace WaterTracker.Features.WaterIntake;

public class WaterIntakeValidator : AbstractValidator<CreateIntakeRequest>
{
    public WaterIntakeValidator()
    {
        RuleFor(x => x.AmountMl).GreaterThan(0).LessThanOrEqualTo(10000);
        RuleFor(x => x.RecordedAt)
            .Must(date => date <= DateTimeOffset.UtcNow.AddMinutes(1))
            .When(x => x.RecordedAt.HasValue) // only validate if client has sent value
            .WithMessage("RecordedAt cannot be more than 1 minute in the future.");
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public class UpdateIntakeValidator : AbstractValidator<UpdateIntakeRequest>
{
    public UpdateIntakeValidator()
    {
        RuleFor(x => x.AmountMl).GreaterThan(0).LessThanOrEqualTo(10000);
        RuleFor(x => x.RecordedAt)
            .Must(date => date <= DateTimeOffset.UtcNow.AddMinutes(1))
            .WithMessage("RecordedAt cannot be more than 1 minute in the future.");
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

namespace WaterTracker.Features.WaterIntake;

public record CreateIntakeRequest(
    int AmountMl,
    DateTimeOffset? RecordedAt,
    string? Notes);

public record UpdateIntakeRequest(
    int AmountMl,
    DateTimeOffset RecordedAt,
    string? Notes
 );

public record IntakeResponse(
    Guid Id,
    string UserId,
    int AmountMl,
    DateTimeOffset RecordedAt,
    string? Notes
 );


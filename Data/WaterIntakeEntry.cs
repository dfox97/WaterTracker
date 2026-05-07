namespace WaterTracker.Data;

public class WaterIntakeEntry
{
    public required Guid Id { get; set; }
    public required string UserId { get; set; }
    public required int AmountMl { get; set; }
    public required DateTimeOffset RecordedAt { get; set; }
    public string? Notes { get; set; }
}

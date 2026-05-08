namespace WaterTracker.Features.WaterIntake;

public interface IWaterIntakeService
{
  Task<IntakeResponse> AddAsync(string userId, CreateIntakeRequest request, CancellationToken ct);

  Task<IEnumerable<IntakeResponse>> GetForUserAsync(string userId, CancellationToken ct);

  Task<IntakeResponse?> UpdateAsync(string userId, Guid entryId, UpdateIntakeRequest request, CancellationToken ct);

  Task<bool> DeleteAsync(string userId, Guid entryId, CancellationToken ct);

}

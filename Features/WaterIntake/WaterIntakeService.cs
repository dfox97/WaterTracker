
using WaterTracker.Data;

namespace WaterTracker.Features.WaterIntake;

public class WaterIntakeService(ApplicationDbContext db) : IWaterIntakeService
{
  public async Task<IntakeResponse> AddAsync(string userId, CreateIntakeRequest request, CancellationToken ct)
    {
        var entry = new WaterIntakeEntry
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AmountMl = request.AmountMl,
            RecordedAt = request.RecordedAt,
            Notes = request.Notes,
        };

        //save db
        db.WaterIntakeEntries.Add(entry);
        await db.SaveChangesAsync(ct);

        return new IntakeResponse(entry.Id, entry.UserId, entry.AmountMl, entry.RecordedAt, entry.Notes);
  }

  public Task<bool> DeleteAsync(string userId, Guid entryId, CancellationToken ct)
  {
    throw new NotImplementedException();
  }

  public Task<IEnumerable<IntakeResponse>> GetForUserAsync(string userId, CancellationToken ct)
  {
    throw new NotImplementedException();
  }

  public Task<IntakeResponse?> UpdateAsync(string userId, Guid entryId, UpdateIntakeRequest request, CancellationToken ct)
  {
    throw new NotImplementedException();
  }
}

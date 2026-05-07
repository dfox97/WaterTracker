using FluentAssertions;
using WaterTracker.Tests.Helpers;

namespace WaterTracker.Tests.Features.WaterIntake;

public class WaterIntakeServiceTests : IDisposable
{
    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;
    private readonly WaterTracker.Data.ApplicationDbContext _db;

    public WaterIntakeServiceTests()
    {
        (_db, _connection) = DbContextFactory.Create();
    }

    [Fact]
    public void DbContext_ShouldStartEmpty()
    {
        _db.WaterIntakeEntries.Should().BeEmpty();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}

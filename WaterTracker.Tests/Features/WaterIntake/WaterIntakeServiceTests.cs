using FluentAssertions;
using WaterTracker.Data;
using WaterTracker.Features.WaterIntake;
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

    [Fact]
    public async Task AddAsync_PersistsEntryOwnedByUser()
    {
        // arrange
        _db.Users.Add(new ApplicationUser
        {
            Id = "user-a",
            UserName = "user-a@test.com",
            Email = "user-a@test.com"
        });
        await _db.SaveChangesAsync();

        var service = new WaterIntakeService(_db);
        var request = new CreateIntakeRequest(250, DateTimeOffset.UtcNow, "morning drink");

        //act
        var result = await service.AddAsync("user-a", request, CancellationToken.None);

        //assert
        result.UserId.Should().Be("user-a");
        result.AmountMl.Should().Be(250);
        _db.WaterIntakeEntries.Should().HaveCount(1);
    }


    [Fact]
    public async Task GetForUserAsync_ReturnsOnlyEntriesForThatUser()
    {
         var user1 = new ApplicationUser { Id = "user-a", UserName = "a@test.com", Email = "a@test.com" };
         var user2 = new ApplicationUser { Id = "user-b", UserName = "b@test.com", Email = "b@test.com" };


        _db.Users.AddRange(user1, user2);


        _db.WaterIntakeEntries.AddRange(
            new WaterIntakeEntry { Id = Guid.NewGuid(), UserId = "user-a", AmountMl = 250, RecordedAt = DateTimeOffset.UtcNow },
            new WaterIntakeEntry { Id = Guid.NewGuid(), UserId = "user-a", AmountMl = 500, RecordedAt = DateTimeOffset.UtcNow },
            new WaterIntakeEntry { Id = Guid.NewGuid(), UserId = "user-b", AmountMl = 300, RecordedAt = DateTimeOffset.UtcNow },
            new WaterIntakeEntry { Id = Guid.NewGuid(), UserId = "user-b", AmountMl = 400, RecordedAt = DateTimeOffset.UtcNow }
         );

        await _db.SaveChangesAsync();


        var service = new WaterIntakeService( _db );

        var result = await service.GetForUserAsync(userId: "user-a", CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.UserId == "user-a");
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}

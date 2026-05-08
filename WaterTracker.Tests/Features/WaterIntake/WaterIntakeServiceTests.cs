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
        await _db.SaveChangesAsync(CancellationToken.None);

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

        await _db.SaveChangesAsync(CancellationToken.None);


        var service = new WaterIntakeService( _db );

        var result = await service.GetForUserAsync(userId: "user-a", CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.UserId == "user-a");
    }

    [Fact]
    public async Task UpdateAsync_ChangeValuesForOwner()
    {
        var user1 = new ApplicationUser { Id = "user-a", UserName = "a@test.com", Email = "a@test.com" };
        _db.Users.Add(user1);
        
        var entryId = Guid.NewGuid();

        _db.WaterIntakeEntries.Add(new WaterIntakeEntry { Id = entryId, UserId = "user-a", AmountMl = 250, RecordedAt = DateTimeOffset.UtcNow, Notes = "original" });
        await _db.SaveChangesAsync(CancellationToken.None);


        var service = new WaterIntakeService(_db);
        var request = new UpdateIntakeRequest(500, DateTimeOffset.UtcNow, "updated");


        var result = await service.UpdateAsync("user-a", entryId, request, CancellationToken.None);


        result.Should().NotBeNull();
        result.AmountMl.Should().Be(500);
        result.Notes.Should().Be("updated");
    }

    [Fact] 
    public async Task UpdateAsync_ReturnsNullForWrongUser()
    { 
        var user1 = new ApplicationUser { Id = "user-a", UserName = "a@test.com", Email = "a@test.com" };
        var user2 = new ApplicationUser { Id = "user-b", UserName = "b@test.com", Email = "b@test.com" };

        _db.Users.AddRange(user1, user2);

        var entryId = Guid.NewGuid();

        _db.WaterIntakeEntries.Add(
            new WaterIntakeEntry
            {
                Id = entryId,
                UserId = "user-a",
                AmountMl = 250,
                RecordedAt = DateTimeOffset.UtcNow,
            });
        await _db.SaveChangesAsync(CancellationToken.None);

        var service = new WaterIntakeService( _db);

        var request = new UpdateIntakeRequest(99999, DateTimeOffset.UtcNow, "bad update");

        var result = await service.UpdateAsync("user-b", entryId, request, CancellationToken.None);

        result.Should().BeNull();
        // check first unchanged
        _db.WaterIntakeEntries.First().AmountMl.Should().Be(250);
    }

    [Fact]
    public async Task DeleteAsync_RemovesEntryForOwner()
    {
        var user1 = new ApplicationUser { Id = "user-a", UserName = "a@test.com", Email = "a@test.com" };
        _db.Users.Add(user1);
        var entryId = Guid.NewGuid();

        _db.WaterIntakeEntries.Add(
            new WaterIntakeEntry
            {
                Id = entryId,
                UserId = "user-a",
                AmountMl = 250,
                RecordedAt = DateTimeOffset.UtcNow,
            });

        await _db.SaveChangesAsync(CancellationToken.None);

        var service = new WaterIntakeService( _db);

       var result = await service.DeleteAsync("user-a", entryId, CancellationToken.None);

       result.Should().BeTrue();
       _db.WaterIntakeEntries.Should().BeEmpty();
    }


    [Fact]
    public async Task DeleteAsync_ReturnsFalseForWrongUser()
    {
        var user1 = new ApplicationUser { Id = "user-a", UserName = "a@test.com", Email = "a@test.com" };
        var user2 = new ApplicationUser { Id = "user-b", UserName = "b@test.com", Email = "b@test.com" };

        _db.Users.AddRange(user1, user2);

        var entryId = Guid.NewGuid();

        _db.WaterIntakeEntries.Add(
            new WaterIntakeEntry
            {
                Id = entryId,
                UserId = "user-a",
                AmountMl = 250,
                RecordedAt = DateTimeOffset.UtcNow,
            });

        await _db.SaveChangesAsync(CancellationToken.None);

        var service = new WaterIntakeService(_db);

        var result = await service.DeleteAsync("user-b", entryId, CancellationToken.None);

        result.Should().BeFalse();
        _db.WaterIntakeEntries.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsEntryForOwner_NullForOther()
    {
        var user1 = new ApplicationUser { Id = "user-a", UserName = "a@test.com", Email = "a@test.com" };
        var user2 = new ApplicationUser { Id = "user-b", UserName = "b@test.com", Email = "b@test.com" };

        _db.Users.AddRange(user1, user2);

        var entryId = Guid.NewGuid();
        _db.WaterIntakeEntries.Add(new WaterIntakeEntry
        {
            Id = entryId,
            UserId = "user-a",
            AmountMl = 250,
            RecordedAt = DateTimeOffset.UtcNow
        });
        await _db.SaveChangesAsync(CancellationToken.None);

        var service = new WaterIntakeService(_db);

        var ownerResult = await service.GetByIdAsync("user-a", entryId, CancellationToken.None);
        var otherResult = await service.GetByIdAsync("user-b", entryId, CancellationToken.None);

        ownerResult.Should().NotBeNull();
        ownerResult.AmountMl.Should().Be(250);
        otherResult.Should().BeNull();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}

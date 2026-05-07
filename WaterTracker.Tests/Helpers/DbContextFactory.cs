using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WaterTracker.Data;

namespace WaterTracker.Tests.Helpers;

public static class DbContextFactory
{
    public static (ApplicationDbContext db, SqliteConnection connection) Create()
    {
        // "Data Source=:memory:" is the SQLite magic string for RAM-only;
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open(); //must stay open - when closed it is wiped.


        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new ApplicationDbContext(options);
        db.Database.EnsureCreated();

        return (db, connection);
    }
}

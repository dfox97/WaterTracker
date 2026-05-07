using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WaterTracker.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<WaterIntakeEntry> WaterIntakeEntries => Set<WaterIntakeEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<WaterIntakeEntry>(entity =>
        {
            entity.HasOne<ApplicationUser>() // 1 user
              .WithMany() // many entries
              .HasForeignKey(e => e.UserId) // tied to user id as fk
              .OnDelete(DeleteBehavior.Cascade); // delete all entries when user is deleted from db

            entity.HasIndex(e => new { e.UserId, e.RecordedAt });
        });
    }
}

using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WaterTracker.Data;

namespace WaterTracker.Tests.Features.WaterIntake;

public class WaterIntakeEndpointTests(WaterTrackerFactory factory)
    : IClassFixture<WaterTrackerFactory>
{
    [Fact]
    public async Task GET_Unauthenticated_Returns401()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/api/intake", CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_InvalidBody_AuthenticatedUser_Returns400()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, "validation-test-user");

        var response = await client.PostAsJsonAsync(
            "/api/intake",
            new { AmountMl = 0, Notes = "bad" },
            CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_ValidBody_AuthenticatedUser_Returns201()
    {
        const string userId = "endpoint-test-user";

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (!db.Users.Any(u => u.Id == userId))
        {
            db.Users.Add(new ApplicationUser
            {
                Id = userId,
                UserName = "ep@test.com",
                Email = "ep@test.com"
            });

            await db.SaveChangesAsync(CancellationToken.None);
        }

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId);

        var response = await client.PostAsJsonAsync(
            "/api/intake",
            new { AmountMl = 500, Notes = "endpoint test" },
            CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        db.WaterIntakeEntries.Should().ContainSingle(e =>
            e.UserId == userId &&
            e.AmountMl == 500 &&
            e.Notes == "endpoint test");
    }
}

public sealed class WaterTrackerFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public WaterTrackerFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (dbContextDescriptor is not null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(_connection));

            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName,
                    _ => { });

            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                options.DefaultScheme = TestAuthHandler.SchemeName;
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }
}

public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";
    public const string UserIdHeader = "X-Test-UserId";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(UserIdHeader, out var userId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        Claim[] claims = [
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, "test@test.com")
        ];

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
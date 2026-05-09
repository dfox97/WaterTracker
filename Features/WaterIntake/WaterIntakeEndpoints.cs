using System.Security.Claims;
using FluentValidation;

namespace WaterTracker.Features.WaterIntake;

public static class WaterIntakeEndpoints
{

    public static void MapWaterIntakeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/intake")
            .RequireAuthorization();

        // Get
        group.MapGet("/", async (IWaterIntakeService srv, ClaimsPrincipal user, CancellationToken ct) =>
        {
            string userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("User ID claim missing.");

            IEnumerable<IntakeResponse> entries = await srv.GetForUserAsync(userId, ct);

            return Results.Ok(entries);
        });

        group.MapGet("/{entryId}", async (IWaterIntakeService srv, Guid entryId, ClaimsPrincipal user, CancellationToken ct) =>
        {
            string userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("User ID claim missing.");

            IntakeResponse? entry = await srv.GetByIdAsync(userId, entryId, ct);

            return entry == null ? Results.NotFound() : Results.Ok(entry);
        });


        group.MapPost("/", async (IWaterIntakeService srv, IValidator<CreateIntakeRequest> validator, CreateIntakeRequest request, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var validation = await validator.ValidateAsync(request, ct);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            string userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("User ID claim missing.");

            IntakeResponse response = await srv.AddAsync(userId, request, ct);

            return Results.Created($"/api/intake/{response.Id}", response);
        });

        group.MapPut("/{entryId}", async (IWaterIntakeService srv, IValidator<UpdateIntakeRequest> validator, Guid entryId, UpdateIntakeRequest request, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var validation = await validator.ValidateAsync(request, ct);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            string userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("User ID claim missing.");

            IntakeResponse? entry = await srv.UpdateAsync(userId, entryId, request, ct);

            return entry == null ? Results.NotFound() : Results.Ok(entry);
        });


        group.MapDelete("/{entryId}", async (IWaterIntakeService srv, Guid entryId, ClaimsPrincipal user, CancellationToken ct) =>
        {
            string userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("User ID claim missing.");

            bool success = await srv.DeleteAsync(userId, entryId, ct);

            return success ? Results.NoContent() : Results.NotFound();
        });
    }
}
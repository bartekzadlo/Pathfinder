using System.Threading;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Pathfinder.Extensions;
using Pathfinder.Modules.Routing.Application;
using Pathfinder.Modules.Routing.Domain;
using Pathfinder.Modules.Gamification.Application;

namespace Pathfinder.Modules.Routing.Api;

public static class RoutingEndpoints
{
    public static IEndpointRouteBuilder MapRoutingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/route", (UserPreferences preferences, RouteGeneratorService routeService, ActivityScoreCalculator gamification, CancellationToken ct) =>
        {
            var plan = routeService.GenerateRoute(preferences);
            var score = gamification.CalculateScore(plan, preferences);
            
            var response = new {
                plan.StartAttraction,
                plan.Segments,
                plan.TotalDistanceKm,
                plan.TotalEstimatedTimeMinutes,
                plan.DebugData,
                plan.UnusedAttractions,
                Gamification = score
            };
            
            return Results.Ok(response);
        })
        .WithName("GenerateRoute")
        .AddEndpointFilter<ValidationFilter<UserPreferences>>();

        endpoints.MapPost("/api/route/recalculate", (RecalculateRequest request, RouteGeneratorService routeService, ActivityScoreCalculator gamification, CancellationToken ct) =>
        {
            var plan = routeService.RecalculateRoute(request.Preferences, request.AttractionIds);
            var score = gamification.CalculateScore(plan, request.Preferences);

            var response = new {
                plan.StartAttraction,
                plan.Segments,
                plan.TotalDistanceKm,
                plan.TotalEstimatedTimeMinutes,
                plan.DebugData,
                plan.UnusedAttractions,
                Gamification = score
            };

            return Results.Ok(response);
        })
        .WithName("RecalculateRoute")
        .AddEndpointFilter<ValidationFilter<RecalculateRequest>>();

        return endpoints;
    }
}

public record RecalculateRequest(UserPreferences Preferences, List<int> AttractionIds);

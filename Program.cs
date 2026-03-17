using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pathfinder.Data;
using Pathfinder.Models;
using Pathfinder.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Register our services
builder.Services.AddSingleton<IAttractionRepository, AttractionRepository>();
builder.Services.AddTransient<RouteGeneratorService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Serve static files from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

// Setup API endpoint
app.MapPost("/api/route", (UserPreferences preferences, RouteGeneratorService routeService) =>
{
    var plan = routeService.GenerateRoute(preferences);
    return Results.Ok(plan);
})
.WithName("GenerateRoute");

app.MapPost("/api/route/recalculate", (RecalculateRequest request, RouteGeneratorService routeService) =>
{
    var plan = routeService.RecalculateRoute(request.Preferences, request.AttractionIds);
    return Results.Ok(plan);
})
.WithName("RecalculateRoute");

app.Run();

// DTO for recalculate request
public record RecalculateRequest(UserPreferences Preferences, List<int> AttractionIds);

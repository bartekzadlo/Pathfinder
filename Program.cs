using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Pathfinder.Extensions;
using Pathfinder.Modules.Routing.Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Register Modular Monolith dependencies
builder.Services.AddPathfinderModules();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Serve static files from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

// Map Modular Endpoints
app.MapRoutingEndpoints();

app.Run();

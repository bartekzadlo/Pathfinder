using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Pathfinder.Modules.Attractions.Domain;
using Pathfinder.Modules.Attractions.Infrastructure;
using Pathfinder.Modules.Attractions.Application;
using Pathfinder.Modules.Routing.Application;
using Pathfinder.Modules.Gamification.Application;
using Pathfinder.Modules.Routing.Application.Validators;

namespace Pathfinder.Extensions;

public static class ModuleExtensions
{
    public static IServiceCollection AddPathfinderModules(this IServiceCollection services)
    {
        // Attractions Module
        services.AddSingleton<IAttractionRepository, InMemoryAttractionRepository>();
        services.AddSingleton<IReservationRepository, InMemoryReservationRepository>();
        services.AddTransient<AttractionService>();
        
        // Routing Module
        services.AddTransient<RouteGeneratorService>();
        
        // Gamification Module
        services.AddTransient<ActivityScoreCalculator>();

        //Validation & Error Handling
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddValidatorsFromAssemblyContaining<UserPreferencesValidator>();
        
        return services;
    }
}

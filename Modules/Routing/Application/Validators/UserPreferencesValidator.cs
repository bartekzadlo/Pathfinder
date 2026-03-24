using FluentValidation;
using Pathfinder.Modules.Routing.Domain;

namespace Pathfinder.Modules.Routing.Application.Validators;

public class UserPreferencesValidator : AbstractValidator<UserPreferences>
{
    public UserPreferencesValidator()
    {
        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .Must(city => city == "Warszawa" || city == "Kraków" || city == "Gdańsk")
            .WithMessage("Currently only Warszawa, Kraków, and Gdańsk are supported.");

        RuleFor(x => x.WalkingDistanceKm)
            .GreaterThan(0).WithMessage("Walking distance must be greater than 0.")
            .LessThanOrEqualTo(50).WithMessage("Walking distance cannot exceed 50 km.");

        RuleFor(x => x.FocusType)
            .InclusiveBetween(1, 10).WithMessage("FocusType must be between 1 and 10.");

        RuleFor(x => x.Weather)
            .Must(w => w == "Sunny" || w == "Cloudy" || w == "Raining")
            .WithMessage("Weather must be Sunny, Cloudy, or Raining.");

        RuleFor(x => x.TransportMode)
            .Must(t => t == "Walking" || t == "PublicTransport" || t == "Car")
            .WithMessage("TransportMode must be Walking, PublicTransport, or Car.");
    }
}

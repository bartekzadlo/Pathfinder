using Pathfinder.Modules.Attractions.Domain;

namespace Pathfinder.Modules.Routing.Domain;

public class RouteSegment
{
    public Attraction ToAttraction { get; private set; }
    public double TravelDistanceKm { get; private set; }
    public int TravelTimeMinutes { get; private set; }
    public string TransportModeIcon { get; private set; }

    public RouteSegment(Attraction toAttraction, double travelDistanceKm, int travelTimeMinutes, string transportModeIcon)
    {
        ToAttraction = toAttraction;
        TravelDistanceKm = travelDistanceKm;
        TravelTimeMinutes = travelTimeMinutes;
        TransportModeIcon = transportModeIcon;
    }
}

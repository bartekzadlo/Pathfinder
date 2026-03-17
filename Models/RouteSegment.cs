namespace Pathfinder.Models;

public class RouteSegment
{
    // The attraction at the end of this segment
    public Attraction ToAttraction { get; set; } = new Attraction();
    
    // Physical distance from the previous attraction to this one
    public double TravelDistanceKm { get; set; }
    
    // Estimated travel time to reach this attraction
    public int TravelTimeMinutes { get; set; }
    
    // Icon/Means of transport used for this segment
    public string TransportModeIcon { get; set; } = "🚶";
}

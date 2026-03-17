using System.Collections.Generic;

namespace Pathfinder.Models;

public class RoutePlan
{
    // The starting attraction (has no prior travel time)
    public Attraction? StartAttraction { get; set; }

    // Subsequent stops with travel data to reach them
    public List<RouteSegment> Segments { get; set; } = new List<RouteSegment>();
    
    // Total distance of the route in km
    public double TotalDistanceKm { get; set; }
    
    // Total estimated time in minutes (travel + duration at attractions)
    public int TotalEstimatedTimeMinutes { get; set; }

    // Debug Data for developers
    public object? DebugData { get; set; }

    // Attractions in the city that were NOT included in the route
    public List<Attraction> UnusedAttractions { get; set; } = new List<Attraction>();
}

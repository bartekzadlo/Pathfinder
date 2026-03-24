using System;
using System.Collections.Generic;
using System.Linq;
using Pathfinder.Modules.Attractions.Domain;

namespace Pathfinder.Modules.Routing.Domain;

// Exception specific for Domain Rules
public class CapacityExceededException : Exception
{
    public CapacityExceededException(string message) : base(message) { }
}

public class RoutePlan
{
    private readonly List<RouteSegment> _segments = new();
    
    public Attraction? StartAttraction { get; private set; }
    public IReadOnlyList<RouteSegment> Segments => _segments.AsReadOnly();
    public double TotalDistanceKm { get; private set; }
    public int TotalEstimatedTimeMinutes { get; private set; }
    public object? DebugData { get; set; }
    public List<Attraction> UnusedAttractions { get; set; } = new();

    // Constant limit for standard day trip (8 hours)
    public const int MaxTripDurationMinutes = 480;

    public void SetStartAttraction(Attraction attraction)
    {
        StartAttraction = attraction;
        TotalEstimatedTimeMinutes += attraction.RecommendedDurationMinutes;
    }

    public bool TryAddSegment(Attraction nextAttraction, double travelDistanceKm, int travelTimeMinutes, string transportIcon, double maxWalkingDistanceKm, bool isWalkingOnly)
    {
        if (isWalkingOnly && TotalDistanceKm + travelDistanceKm > maxWalkingDistanceKm)
            return false;

        if (TotalEstimatedTimeMinutes + travelTimeMinutes + nextAttraction.RecommendedDurationMinutes > MaxTripDurationMinutes)
            return false;

        _segments.Add(new RouteSegment(nextAttraction, Math.Round(travelDistanceKm, 2), travelTimeMinutes, transportIcon));
        TotalDistanceKm += travelDistanceKm;
        TotalEstimatedTimeMinutes += (travelTimeMinutes + nextAttraction.RecommendedDurationMinutes);
        return true;
    }

    public void RecalculateTotalDistance()
    {
        TotalDistanceKm = Math.Round(TotalDistanceKm, 2);
    }
}

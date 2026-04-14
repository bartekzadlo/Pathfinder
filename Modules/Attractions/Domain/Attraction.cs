using System;

namespace Pathfinder.Modules.Attractions.Domain;

// Aggregate Root for Attraction
public class Attraction
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public GeographicCoordinates Coordinates { get; private set; }
    public bool IsOutdoor { get; private set; }
    
    public int ExplorationScore { get; private set; }
    public int RelaxationScore { get; private set; }
    public int RecommendedDurationMinutes { get; private set; }
    
    public Season Season { get; private set; }
    public bool IsAccessibleForDisabled { get; private set; }
    public int MaxConcurrentReservations { get; private set; }

    public Attraction(
        int id, string name, string city, 
        double latitude, double longitude, 
        bool isOutdoor, int explorationScore, 
        int relaxationScore, int recommendedDurationMinutes,
        Season season, bool isAccessibleForDisabled, int maxConcurrentReservations)
    {
        Id = id;
        Name = name;
        City = city;
        Coordinates = new GeographicCoordinates(latitude, longitude);
        IsOutdoor = isOutdoor;
        ExplorationScore = explorationScore;
        RelaxationScore = relaxationScore;
        RecommendedDurationMinutes = recommendedDurationMinutes;
        Season = season;
        IsAccessibleForDisabled = isAccessibleForDisabled;
        MaxConcurrentReservations = maxConcurrentReservations;
    }

    // Default constructor for serialization/mocking purposes if needed, though pure DDD avoids it.
    private Attraction() { }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Pathfinder.Modules.Attractions.Domain;
using Pathfinder.Modules.Attractions.Infrastructure;
using Pathfinder.Modules.Routing.Domain;

namespace Pathfinder.Modules.Routing.Application;

public class RouteGeneratorService
{
    private readonly IAttractionRepository _attractionRepository;

    public RouteGeneratorService(IAttractionRepository attractionRepository)
    {
        _attractionRepository = attractionRepository;
    }

    public RoutePlan GenerateRoute(UserPreferences preferences)
    {
        var allAttractions = _attractionRepository.GetAllAttractions();

        if (preferences.Weather.Equals("Raining", StringComparison.OrdinalIgnoreCase))
            allAttractions = allAttractions.Where(a => !a.IsOutdoor).ToList();

        allAttractions = allAttractions.Where(a => a.City.Equals(preferences.City, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!allAttractions.Any()) return new RoutePlan();

        double exploreWeight = preferences.FocusType / 10.0;
        double relaxWeight = (11 - preferences.FocusType) / 10.0;

        var scoredAttractions = allAttractions.Select(a => new
        {
            Attraction = a,
            Score = (a.ExplorationScore * exploreWeight) + (a.RelaxationScore * relaxWeight)
        }).OrderByDescending(x => x.Score).ToList();

        double speedKmPerHour = preferences.TransportMode == "PublicTransport" ? 15.0 :
                                preferences.TransportMode == "Car" ? 25.0 : 5.0;
        string transportIcon = preferences.TransportMode == "PublicTransport" ? "🚌" :
                               preferences.TransportMode == "Car" ? "🚗" : "🚶";

        double speedKmPerMin = speedKmPerHour / 60.0;

        var plan = new RoutePlan();
        var currentAttraction = scoredAttractions.First().Attraction;
        plan.SetStartAttraction(currentAttraction);

        var remainingPool = scoredAttractions.Skip(1).Select(x => x.Attraction).ToList();
        bool isWalkingOnly = preferences.TransportMode == "Walking";

        while (remainingPool.Any())
        {
            Attraction? bestNext = null;
            double shortestDistance = double.MaxValue;

            foreach (var candidate in remainingPool)
            {
                double dist = currentAttraction.Coordinates.DistanceTo(candidate.Coordinates);

                if (dist < shortestDistance)
                {
                    shortestDistance = dist;
                    bestNext = candidate;
                }
            }

            if (bestNext == null) break;

            int travelTimeMins = (int)Math.Ceiling(shortestDistance / speedKmPerMin);

            // Delegate invariant checks to AggregateRoot
            bool success = plan.TryAddSegment(bestNext, shortestDistance, travelTimeMins, transportIcon, preferences.WalkingDistanceKm, isWalkingOnly);
            
            if (!success) break;

            remainingPool.Remove(bestNext);
            currentAttraction = bestNext;
        }

        plan.RecalculateTotalDistance();

        plan.DebugData = new
        {
            Preferences = preferences,
            AllScoredAttractions = scoredAttractions.Select(sa => new
            {
                sa.Attraction.Id,
                sa.Attraction.Name,
                OriginalExploration = sa.Attraction.ExplorationScore,
                OriginalRelaxation = sa.Attraction.RelaxationScore,
                CalculatedScore = sa.Score,
                sa.Attraction.IsOutdoor
            }),
            CalculatedExploreWeight = exploreWeight,
            CalculatedRelaxWeight = relaxWeight,
            TransportAssumptions = new { SpeedKmPerHour = speedKmPerHour, BaseIcon = transportIcon, SegmentCount = plan.Segments.Count }
        };

        plan.UnusedAttractions = remainingPool.ToList();

        return plan;
    }

    public RoutePlan RecalculateRoute(UserPreferences preferences, List<int> orderedAttractionIds)
    {
        var allAttractions = _attractionRepository.GetAllAttractions();

        if (preferences.Weather.Equals("Raining", StringComparison.OrdinalIgnoreCase))
            allAttractions = allAttractions.Where(a => !a.IsOutdoor).ToList();

        allAttractions = allAttractions.Where(a => a.City.Equals(preferences.City, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!allAttractions.Any() || !orderedAttractionIds.Any()) return new RoutePlan();

        var selectedAttractions = orderedAttractionIds
            .Select(id => allAttractions.FirstOrDefault(a => a.Id == id))
            .Where(a => a != null)
            .Cast<Attraction>()
            .ToList();

        if (!selectedAttractions.Any()) return new RoutePlan();

        double speedKmPerHour = preferences.TransportMode == "PublicTransport" ? 15.0 :
                                preferences.TransportMode == "Car" ? 25.0 : 5.0;
        string transportIcon = preferences.TransportMode == "PublicTransport" ? "🚌" :
                               preferences.TransportMode == "Car" ? "🚗" : "🚶";

        double speedKmPerMin = speedKmPerHour / 60.0;
        var plan = new RoutePlan();
        
        var currentAttraction = selectedAttractions.First();
        plan.SetStartAttraction(currentAttraction);

        bool isWalkingOnly = preferences.TransportMode == "Walking";

        foreach (var nextAttraction in selectedAttractions.Skip(1))
        {
            double dist = currentAttraction.Coordinates.DistanceTo(nextAttraction.Coordinates);
            int travelTimeMins = (int)Math.Ceiling(dist / speedKmPerMin);

            plan.TryAddSegment(nextAttraction, dist, travelTimeMins, transportIcon, preferences.WalkingDistanceKm, isWalkingOnly);
            currentAttraction = nextAttraction;
        }

        plan.RecalculateTotalDistance();

        var usedIds = new HashSet<int>(selectedAttractions.Select(a => a.Id));
        plan.UnusedAttractions = allAttractions.Where(a => !usedIds.Contains(a.Id)).ToList();

        return plan;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Pathfinder.Models;
using Pathfinder.Data;

namespace Pathfinder.Services;

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
        {
            allAttractions = allAttractions.Where(a => !a.IsOutdoor).ToList();
        }

        allAttractions = allAttractions.Where(a => a.City.Equals(preferences.City, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!allAttractions.Any())
        {
            return new RoutePlan();
        }

        double exploreWeight = (preferences.FocusType) / 10.0;
        double relaxWeight = (11 - preferences.FocusType) / 10.0;

        var scoredAttractions = allAttractions.Select(a => new
        {
            Attraction = a,
            Score = (a.ExplorationScore * exploreWeight) + (a.RelaxationScore * relaxWeight)
        }).OrderByDescending(x => x.Score).ToList();

        double speedKmPerHour = 5.0;
        string transportIcon = "🚶";

        if (preferences.TransportMode == "PublicTransport")
        {
            speedKmPerHour = 15.0;
            transportIcon = "🚌";
        }
        else if (preferences.TransportMode == "Car")
        {
            speedKmPerHour = 25.0;
            transportIcon = "🚗";
        }

        double speedKmPerMin = speedKmPerHour / 60.0;

        var plan = new RoutePlan();

        var currentAttraction = scoredAttractions.First().Attraction;
        plan.StartAttraction = currentAttraction;
        plan.TotalEstimatedTimeMinutes += currentAttraction.RecommendedDurationMinutes;

        var remainingPool = scoredAttractions.Skip(1).Select(x => x.Attraction).ToList();

        while (remainingPool.Any())
        {
            Attraction? bestNext = null;
            double shortestDistance = double.MaxValue;

            foreach (var candidate in remainingPool)
            {
                double dist = CalculateDistance(
                    currentAttraction.Latitude, currentAttraction.Longitude,
                    candidate.Latitude, candidate.Longitude);

                if (dist < shortestDistance)
                {
                    shortestDistance = dist;
                    bestNext = candidate;
                }
            }

            if (bestNext == null) break;

            if (plan.TotalDistanceKm + shortestDistance > preferences.WalkingDistanceKm && preferences.TransportMode == "Walking")
            {
                break;
            }

            int travelTimeMins = (int)Math.Ceiling(shortestDistance / speedKmPerMin);

            if (plan.TotalEstimatedTimeMinutes + travelTimeMins + bestNext.RecommendedDurationMinutes > 480)
            {
                break;
            }

            plan.TotalDistanceKm += shortestDistance;

            plan.Segments.Add(new RouteSegment
            {
                ToAttraction = bestNext,
                TravelDistanceKm = Math.Round(shortestDistance, 2),
                TravelTimeMinutes = travelTimeMins,
                TransportModeIcon = transportIcon
            });

            plan.TotalEstimatedTimeMinutes += (travelTimeMins + bestNext.RecommendedDurationMinutes);

            remainingPool.Remove(bestNext);
            currentAttraction = bestNext;
        }

        plan.TotalDistanceKm = Math.Round(plan.TotalDistanceKm, 2);

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
            TransportAssumptions = new
            {
                SpeedKmPerHour = speedKmPerHour,
                BaseIcon = transportIcon,
                SegmentCount = plan.Segments.Count
            }
        };

        plan.UnusedAttractions = remainingPool.ToList();

        return plan;
    }

    public RoutePlan RecalculateRoute(UserPreferences preferences, List<int> orderedAttractionIds)
    {
        var allAttractions = _attractionRepository.GetAllAttractions();

        if (preferences.Weather.Equals("Raining", StringComparison.OrdinalIgnoreCase))
        {
            allAttractions = allAttractions.Where(a => !a.IsOutdoor).ToList();
        }

        allAttractions = allAttractions.Where(a => a.City.Equals(preferences.City, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!allAttractions.Any() || !orderedAttractionIds.Any())
        {
            return new RoutePlan();
        }

        var selectedAttractions = orderedAttractionIds
            .Select(id => allAttractions.FirstOrDefault(a => a.Id == id))
            .Where(a => a != null)
            .Cast<Attraction>()
            .ToList();

        if (!selectedAttractions.Any())
        {
            return new RoutePlan();
        }

        double speedKmPerHour = 5.0;
        string transportIcon = "🚶";

        if (preferences.TransportMode == "PublicTransport") 
        {
            speedKmPerHour = 15.0;
            transportIcon = "🚌";
        }
        else if (preferences.TransportMode == "Car") 
        {
            speedKmPerHour = 25.0;
            transportIcon = "🚗";
        }

        double speedKmPerMin = speedKmPerHour / 60.0;
        var plan = new RoutePlan();

        var currentAttraction = selectedAttractions.First();
        plan.StartAttraction = currentAttraction;
        plan.TotalEstimatedTimeMinutes += currentAttraction.RecommendedDurationMinutes;

        foreach (var nextAttraction in selectedAttractions.Skip(1))
        {
            double dist = CalculateDistance(
                currentAttraction.Latitude, currentAttraction.Longitude,
                nextAttraction.Latitude, nextAttraction.Longitude);

            int travelTimeMins = (int)Math.Ceiling(dist / speedKmPerMin);

            plan.TotalDistanceKm += dist;
            plan.Segments.Add(new RouteSegment
            {
                ToAttraction = nextAttraction,
                TravelDistanceKm = Math.Round(dist, 2),
                TravelTimeMinutes = travelTimeMins,
                TransportModeIcon = transportIcon
            });

            plan.TotalEstimatedTimeMinutes += (travelTimeMins + nextAttraction.RecommendedDurationMinutes);
            currentAttraction = nextAttraction;
        }

        plan.TotalDistanceKm = Math.Round(plan.TotalDistanceKm, 2);

        var usedIds = new HashSet<int>(selectedAttractions.Select(a => a.Id));
        plan.UnusedAttractions = allAttractions.Where(a => !usedIds.Contains(a.Id)).ToList();

        return plan;
    }

    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var R = 6371d;
        var dLat = Deg2Rad(lat2 - lat1);
        var dLon = Deg2Rad(lon2 - lon1);
        var a =
            Math.Sin(dLat / 2d) * Math.Sin(dLat / 2d) +
            Math.Cos(Deg2Rad(lat1)) * Math.Cos(Deg2Rad(lat2)) *
            Math.Sin(dLon / 2d) * Math.Sin(dLon / 2d);
        var c = 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1d - a));
        var d = R * c;
        return d;
    }

    private double Deg2Rad(double deg)
    {
        return deg * (Math.PI / 180d);
    }
}

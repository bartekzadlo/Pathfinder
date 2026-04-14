using System;
using System.Collections.Generic;
using System.Linq;
using Pathfinder.Modules.Attractions.Domain;

namespace Pathfinder.Modules.Attractions.Application;

public class AttractionService
{
    private readonly IAttractionRepository _attractionRepository;
    private readonly IReservationRepository _reservationRepository;

    public AttractionService(
        IAttractionRepository attractionRepository, 
        IReservationRepository reservationRepository)
    {
        _attractionRepository = attractionRepository;
        _reservationRepository = reservationRepository;
    }

    public Dictionary<Season, List<Attraction>> GetAttractionsGroupedBySeason()
    {
        var attractions = _attractionRepository.GetAllAttractions();
        
        return attractions
            .GroupBy(a => a.Season)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    public List<Attraction> GetAccessibleAttractions()
    {
        return _attractionRepository.GetAllAttractions()
            .Where(a => a.IsAccessibleForDisabled)
            .ToList();
    }

    public Reservation MakeReservation(int attractionId, DateTime startTime, DateTime endTime)
    {
        var attraction = _attractionRepository.GetAllAttractions()
            .FirstOrDefault(a => a.Id == attractionId);

        if (attraction == null)
        {
            throw new ArgumentException($"Attraction with ID {attractionId} not found.");
        }

        var overlappingReservations = _reservationRepository.GetOverlappingReservations(attractionId, startTime, endTime);

        if (overlappingReservations.Count >= attraction.MaxConcurrentReservations)
        {
            throw new InvalidOperationException($"Cannot make reservation. The maximum capacity of {attraction.MaxConcurrentReservations} has been reached for the selected time.");
        }

        var reservation = new Reservation(Guid.NewGuid(), attractionId, startTime, endTime);
        _reservationRepository.Add(reservation);

        return reservation;
    }
}

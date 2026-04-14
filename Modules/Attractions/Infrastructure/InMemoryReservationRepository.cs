using System;
using System.Collections.Generic;
using System.Linq;
using Pathfinder.Modules.Attractions.Domain;

namespace Pathfinder.Modules.Attractions.Infrastructure;

public class InMemoryReservationRepository : IReservationRepository
{
    private readonly List<Reservation> _reservations = new();

    public void Add(Reservation reservation)
    {
        _reservations.Add(reservation);
    }

    public List<Reservation> GetByAttractionId(int attractionId)
    {
        return _reservations.Where(r => r.AttractionId == attractionId).ToList();
    }

    public List<Reservation> GetOverlappingReservations(int attractionId, DateTime startTime, DateTime endTime)
    {
        return _reservations
            .Where(r => r.AttractionId == attractionId && r.StartTime < endTime && r.EndTime > startTime)
            .ToList();
    }
}

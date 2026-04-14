using System;
using System.Collections.Generic;

namespace Pathfinder.Modules.Attractions.Domain;

public interface IReservationRepository
{
    void Add(Reservation reservation);
    List<Reservation> GetByAttractionId(int attractionId);
    List<Reservation> GetOverlappingReservations(int attractionId, DateTime startTime, DateTime endTime);
}

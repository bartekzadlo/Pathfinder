using System;

namespace Pathfinder.Modules.Attractions.Domain;

public class Reservation
{
    public Guid Id { get; private set; }
    public int AttractionId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }

    public Reservation(Guid id, int attractionId, DateTime startTime, DateTime endTime)
    {
        if (endTime <= startTime)
        {
            throw new ArgumentException("EndTime must be greater than StartTime.");
        }

        Id = id;
        AttractionId = attractionId;
        StartTime = startTime;
        EndTime = endTime;
    }
}

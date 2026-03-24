using System;

namespace Pathfinder.Modules.Attractions.Domain;

public record GeographicCoordinates
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }

    public GeographicCoordinates(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public double DistanceTo(GeographicCoordinates other)
    {
        var r = 6371d; // Earth radius in km
        var dLat = Deg2Rad(other.Latitude - Latitude);
        var dLon = Deg2Rad(other.Longitude - Longitude);
        
        var a = Math.Sin(dLat / 2d) * Math.Sin(dLat / 2d) +
                Math.Cos(Deg2Rad(Latitude)) * Math.Cos(Deg2Rad(other.Latitude)) *
                Math.Sin(dLon / 2d) * Math.Sin(dLon / 2d);
                
        var c = 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1d - a));
        return r * c;
    }

    private static double Deg2Rad(double deg) => deg * (Math.PI / 180d);
}

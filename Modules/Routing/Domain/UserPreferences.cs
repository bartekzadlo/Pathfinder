namespace Pathfinder.Modules.Routing.Domain;

public class UserPreferences
{
    public string City { get; set; } = "Warszawa";
    public double WalkingDistanceKm { get; set; }
    public string TransportMode { get; set; } = "Walking";
    public int FocusType { get; set; }
    public string Weather { get; set; } = "Sunny";
}

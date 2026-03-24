using System;
using Pathfinder.Modules.Gamification.Domain;
using Pathfinder.Modules.Routing.Domain;

namespace Pathfinder.Modules.Gamification.Application;

public class ActivityScoreCalculator
{
    // A walking person burns roughly 60 kcal per km.
    private const int KcalPerKmWalking = 60;
    
    // 50 XP per km of walking
    private const int XpPerKmWalking = 50;

    public GamificationResult CalculateScore(RoutePlan plan, UserPreferences preferences)
    {
        if (preferences.TransportMode != "Walking")
        {
            return new GamificationResult(plan.TotalDistanceKm, 0, 0, "No XP gained because you used transport.");
        }

        var distance = plan.TotalDistanceKm;
        int calories = (int)Math.Round(distance * KcalPerKmWalking);
        int xp = (int)Math.Round(distance * XpPerKmWalking);

        string message = "Good job! Keep exploring on foot!";
        
        if (xp > 500)
            message = "Unstoppable! You gained a massive amount of XP!";
        else if (preferences.Weather.Equals("Raining", StringComparison.OrdinalIgnoreCase))
        {
            // Bonus for walking in the rain
            xp = (int)(xp * 1.5);
            message = "Hardcore Explorer! +50% XP for walking in the rain!";
        }

        return new GamificationResult(distance, calories, xp, message);
    }
}

namespace Pathfinder.Modules.Gamification.Domain;

public record GamificationResult(
    double DistanceKm,
    int BurnedCalories,
    int ExperiencePointsGained,
    string AchievementMessage
);

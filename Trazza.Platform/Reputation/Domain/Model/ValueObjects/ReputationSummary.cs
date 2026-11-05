namespace Trazza.Platform.Reputation.Domain.Model.ValueObjects;

public record ReputationSummary(int UserId, double AverageScore, int TotalRatings, int PositiveRatings);
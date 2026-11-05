namespace Trazza.Platform.Reputation.Interfaces.Rest.Resources;

public record ReputationSummaryResource(int UserId, double AverageScore, int TotalRatings, int PositiveRatings);
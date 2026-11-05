namespace Trazza.Platform.Reputation.Interfaces.Rest.Resources;

public record RatingResource(
    int Id,
    int ShipmentId,
    int RaterId,
    string RaterName,
    int TargetId,
    string TargetRole,
    string TargetName,
    int Score,
    IEnumerable<string> Tags,
    string Comment,
    DateTimeOffset? CreatedAt);
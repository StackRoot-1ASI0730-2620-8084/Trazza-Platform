namespace Trazza.Platform.Reputation.Interfaces.Rest.Resources;

public record SubmitRatingResource(
    int ShipmentId,
    int RaterId,
    int Score,
    IEnumerable<string>? Tags,
    string? Comment);
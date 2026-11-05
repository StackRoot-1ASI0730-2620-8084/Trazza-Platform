using Trazza.Platform.Reputation.Domain.Model.ValueObjects;

namespace Trazza.Platform.Reputation.Domain.Model.Commands;

public record SubmitRatingCommand(
    ShipmentId ShipmentId,
    RaterId RaterId,
    int Score,
    IReadOnlyList<string> Tags,
    string Comment);
using Trazza.Platform.Shared.Domain.Model.Errors;

namespace Trazza.Platform.Reputation.Domain.Model.Errors;

public static class ReputationErrors
{
    public static readonly Error RatingNotFound =
        Error.NotFound("Reputation.RatingNotFound", "The specified rating was not found.");

    public static readonly Error ShipmentNotFound =
        Error.NotFound("Reputation.ShipmentNotFound", "The rated shipment was not found.");

    public static readonly Error ShipmentNotDelivered =
        Error.Conflict("Reputation.ShipmentNotDelivered", "Only delivered shipments can be rated.");

    public static readonly Error NotAParty =
        Error.Forbidden("Reputation.NotAParty", "Only the parties of the shipment can rate it.");

    public static readonly Error AlreadyRated =
        Error.Conflict("Reputation.AlreadyRated", "You already rated this shipment.");

    public static readonly Error InvalidScore =
        Error.Validation("Reputation.InvalidScore", "The score must be an integer between 1 and 5.");

    public static readonly Error InvalidTag =
        Error.Validation("Reputation.InvalidTag", "One or more tags are not allowed for the rated party.");

    public static readonly Error CommentTooLong =
        Error.Validation("Reputation.CommentTooLong", "The comment must have at most 300 characters.");
}
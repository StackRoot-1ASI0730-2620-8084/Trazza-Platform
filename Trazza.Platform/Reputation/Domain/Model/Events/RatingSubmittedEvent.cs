using Trazza.Platform.Reputation.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.Events;

namespace Trazza.Platform.Reputation.Domain.Model.Events;

public record RatingSubmittedEvent(RatedUserId RatedUserId, ShipmentId ShipmentId, int Score) : IEvent;
using Trazza.Platform.Reputation.Domain.Model.Events;
using Trazza.Platform.Reputation.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.Aggregates.Internal;

namespace Trazza.Platform.Reputation.Domain.Model.Aggregates;

public partial class Rating : AggregateRoot
{
    public const int MaxCommentLength = 300;

    public Rating()
    {
        RaterName = string.Empty;
        Target = new RatingTarget();
        Score = new Score();
        Tags = [];
        Comment = string.Empty;
    }

    public Rating(ShipmentId shipmentId, RaterId raterId, string raterName, RatingTarget target, Score score,
        IEnumerable<string> tags, string comment) : this()
    {
        ShipmentId = shipmentId;
        RaterId = raterId;
        RaterName = raterName;
        Target = target;
        Score = score;
        Tags = tags.Distinct().ToList();
        Comment = comment.Trim();
        AddDomainEvent(new RatingSubmittedEvent(target.UserId, shipmentId, score.Value));
    }

    public RatingId Id { get; }
    public ShipmentId ShipmentId { get; private set; }
    public RaterId RaterId { get; private set; }
    public string RaterName { get; private set; }
    public RatingTarget Target { get; private set; }
    public Score Score { get; private set; }
    public List<string> Tags { get; private set; }
    public string Comment { get; private set; }
}
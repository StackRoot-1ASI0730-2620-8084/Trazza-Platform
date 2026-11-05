using Trazza.Platform.Reputation.Domain.Model.Aggregates;
using Trazza.Platform.Reputation.Interfaces.Rest.Resources;

namespace Trazza.Platform.Reputation.Interfaces.Rest.Transform;

public static class RatingResourceFromEntityAssembler
{
    public static RatingResource ToResourceFromEntity(Rating rating)
    {
        return new RatingResource(rating.Id, rating.ShipmentId, rating.RaterId, rating.RaterName,
            rating.Target.UserId, rating.Target.Role, rating.Target.Name, rating.Score.Value, rating.Tags.ToList(),
            rating.Comment, rating.CreatedAt);
    }
}
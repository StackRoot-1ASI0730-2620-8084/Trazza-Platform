using Trazza.Platform.Reputation.Domain.Model.Commands;
using Trazza.Platform.Reputation.Domain.Model.ValueObjects;
using Trazza.Platform.Reputation.Interfaces.Rest.Resources;

namespace Trazza.Platform.Reputation.Interfaces.Rest.Transform;

public static class SubmitRatingCommandFromResourceAssembler
{
    public static SubmitRatingCommand ToCommandFromResource(SubmitRatingResource resource)
    {
        return new SubmitRatingCommand(new ShipmentId(resource.ShipmentId), new RaterId(resource.RaterId),
            resource.Score, (resource.Tags ?? []).ToList(), resource.Comment ?? string.Empty);
    }
}
using Trazza.Platform.Reputation.Domain.Model.ValueObjects;
using Trazza.Platform.Reputation.Interfaces.Rest.Resources;

namespace Trazza.Platform.Reputation.Interfaces.Rest.Transform;

public static class ReputationSummaryResourceFromValueObjectAssembler
{
    public static ReputationSummaryResource ToResourceFromValueObject(ReputationSummary summary)
    {
        return new ReputationSummaryResource(summary.UserId, summary.AverageScore, summary.TotalRatings,
            summary.PositiveRatings);
    }
}
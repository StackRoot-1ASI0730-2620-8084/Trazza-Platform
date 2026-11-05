using Trazza.Platform.Reputation.Domain.Model.Aggregates;
using Trazza.Platform.Reputation.Domain.Model.Queries;
using Trazza.Platform.Reputation.Domain.Model.ValueObjects;

namespace Trazza.Platform.Reputation.Application.QueryServices;

public interface IRatingQueryService
{
    Task<IEnumerable<Rating>> Handle(GetAllRatingsQuery query, CancellationToken cancellationToken);

    Task<Rating?> Handle(GetRatingByIdQuery query, CancellationToken cancellationToken);

    Task<IEnumerable<Rating>> Handle(GetRatingsByRatedUserIdQuery query, CancellationToken cancellationToken);

    Task<IEnumerable<Rating>> Handle(GetRatingsByRaterIdQuery query, CancellationToken cancellationToken);

    Task<IEnumerable<Rating>> Handle(GetRatingsByShipmentIdQuery query, CancellationToken cancellationToken);

    Task<ReputationSummary> Handle(GetReputationSummaryQuery query, CancellationToken cancellationToken);
}
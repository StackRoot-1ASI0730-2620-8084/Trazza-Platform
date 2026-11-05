using Trazza.Platform.Reputation.Application.QueryServices;
using Trazza.Platform.Reputation.Domain.Model.Aggregates;
using Trazza.Platform.Reputation.Domain.Model.Queries;
using Trazza.Platform.Reputation.Domain.Model.ValueObjects;
using Trazza.Platform.Reputation.Domain.Repositories;

namespace Trazza.Platform.Reputation.Application.Internal.QueryServices;

public class RatingQueryService(IRatingRepository ratingRepository) : IRatingQueryService
{
    public async Task<IEnumerable<Rating>> Handle(GetAllRatingsQuery query, CancellationToken cancellationToken)
    {
        return await ratingRepository.ListAsNoTrackingAsync(cancellationToken);
    }

    public async Task<Rating?> Handle(GetRatingByIdQuery query, CancellationToken cancellationToken)
    {
        return await ratingRepository.FindByIdAsNoTrackingAsync(query.RatingId, cancellationToken);
    }

    public async Task<IEnumerable<Rating>> Handle(GetRatingsByRatedUserIdQuery query,
        CancellationToken cancellationToken)
    {
        return await ratingRepository.FindByRatedUserIdAsNoTrackingAsync(query.RatedUserId, cancellationToken);
    }

    public async Task<IEnumerable<Rating>> Handle(GetRatingsByRaterIdQuery query, CancellationToken cancellationToken)
    {
        return await ratingRepository.FindByRaterIdAsNoTrackingAsync(query.RaterId, cancellationToken);
    }

    public async Task<IEnumerable<Rating>> Handle(GetRatingsByShipmentIdQuery query,
        CancellationToken cancellationToken)
    {
        return await ratingRepository.FindByShipmentIdAsNoTrackingAsync(query.ShipmentId, cancellationToken);
    }

    public async Task<ReputationSummary> Handle(GetReputationSummaryQuery query, CancellationToken cancellationToken)
    {
        var ratings = (await ratingRepository.FindByRatedUserIdAsNoTrackingAsync(query.RatedUserId,
            cancellationToken)).ToList();
        var average = ratings.Count == 0
            ? 0
            : Math.Round(ratings.Average(rating => rating.Score.Value), 1, MidpointRounding.AwayFromZero);
        return new ReputationSummary(query.RatedUserId, average, ratings.Count,
            ratings.Count(rating => rating.Score.IsPositive));
    }
}

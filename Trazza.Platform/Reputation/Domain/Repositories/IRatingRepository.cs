using Trazza.Platform.Reputation.Domain.Model.Aggregates;
using Trazza.Platform.Reputation.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Repositories;

namespace Trazza.Platform.Reputation.Domain.Repositories;

public interface IRatingRepository : IBaseRepository<Rating>
{
    Task<IEnumerable<Rating>> FindByRatedUserIdAsNoTrackingAsync(RatedUserId ratedUserId,
        CancellationToken cancellationToken);

    Task<IEnumerable<Rating>> FindByRaterIdAsNoTrackingAsync(RaterId raterId, CancellationToken cancellationToken);

    Task<IEnumerable<Rating>> FindByShipmentIdAsNoTrackingAsync(ShipmentId shipmentId,
        CancellationToken cancellationToken);

    Task<bool> ExistsByShipmentIdAndRaterIdAsync(ShipmentId shipmentId, RaterId raterId,
        CancellationToken cancellationToken);
}
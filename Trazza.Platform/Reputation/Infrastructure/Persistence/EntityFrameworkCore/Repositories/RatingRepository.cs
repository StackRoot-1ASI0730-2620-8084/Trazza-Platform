using Trazza.Platform.Reputation.Domain.Model.Aggregates;
using Trazza.Platform.Reputation.Domain.Model.ValueObjects;
using Trazza.Platform.Reputation.Domain.Repositories;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Trazza.Platform.Reputation.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class RatingRepository(AppDbContext context) : BaseRepository<Rating>(context), IRatingRepository
{
    public async Task<IEnumerable<Rating>> FindByRatedUserIdAsNoTrackingAsync(RatedUserId ratedUserId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<Rating>()
            .AsNoTracking()
            .Where(rating => rating.Target.UserId == ratedUserId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Rating>> FindByRaterIdAsNoTrackingAsync(RaterId raterId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<Rating>()
            .AsNoTracking()
            .Where(rating => rating.RaterId == raterId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Rating>> FindByShipmentIdAsNoTrackingAsync(ShipmentId shipmentId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<Rating>()
            .AsNoTracking()
            .Where(rating => rating.ShipmentId == shipmentId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByShipmentIdAndRaterIdAsync(ShipmentId shipmentId, RaterId raterId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<Rating>()
            .AnyAsync(rating => rating.ShipmentId == shipmentId && rating.RaterId == raterId, cancellationToken);
    }
}
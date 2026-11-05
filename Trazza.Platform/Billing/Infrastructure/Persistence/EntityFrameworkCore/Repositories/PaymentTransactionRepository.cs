using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Billing.Domain.Repositories;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Trazza.Platform.Billing.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class PaymentTransactionRepository(AppDbContext context)
    : BaseRepository<PaymentTransaction>(context), IPaymentTransactionRepository
{
    public async Task<IEnumerable<PaymentTransaction>> FindByUserIdAsNoTrackingAsync(UserId userId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<PaymentTransaction>()
            .AsNoTracking()
            .Where(transaction => transaction.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PaymentTransaction>> FindPaidByUserIdAsNoTrackingAsync(UserId userId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<PaymentTransaction>()
            .AsNoTracking()
            .Where(transaction => transaction.UserId == userId && transaction.Status == EPaymentStatus.Paid)
            .ToListAsync(cancellationToken);
    }
}

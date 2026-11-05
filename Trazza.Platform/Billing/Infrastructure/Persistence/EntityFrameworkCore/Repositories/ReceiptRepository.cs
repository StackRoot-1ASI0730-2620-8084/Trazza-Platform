using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Billing.Domain.Repositories;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Trazza.Platform.Billing.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class ReceiptRepository(AppDbContext context) : BaseRepository<Receipt>(context), IReceiptRepository
{
    public async Task<IEnumerable<Receipt>> FindByUserIdAsNoTrackingAsync(UserId userId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<Receipt>()
            .AsNoTracking()
            .Where(receipt => receipt.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> FindLastNumberByTypeAsync(ReceiptType type, CancellationToken cancellationToken)
    {
        return await Context.Set<Receipt>()
            .Where(receipt => receipt.Type == type)
            .Select(receipt => (int?)receipt.Number)
            .MaxAsync(cancellationToken) ?? 0;
    }

    public async Task<bool> ExistsByTransactionIdAsync(PaymentTransactionId transactionId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<Receipt>().AnyAsync(receipt => receipt.TransactionId == transactionId,
            cancellationToken);
    }
}

using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Repositories;

namespace Trazza.Platform.Billing.Domain.Repositories;

public interface IReceiptRepository : IBaseRepository<Receipt>
{
    Task<IEnumerable<Receipt>> FindByUserIdAsNoTrackingAsync(UserId userId, CancellationToken cancellationToken);

    Task<int> FindLastNumberByTypeAsync(ReceiptType type, CancellationToken cancellationToken);

    Task<bool> ExistsByTransactionIdAsync(PaymentTransactionId transactionId, CancellationToken cancellationToken);
}

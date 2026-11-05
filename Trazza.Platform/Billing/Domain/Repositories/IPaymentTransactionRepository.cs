using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Repositories;

namespace Trazza.Platform.Billing.Domain.Repositories;

public interface IPaymentTransactionRepository : IBaseRepository<PaymentTransaction>
{
    Task<IEnumerable<PaymentTransaction>> FindByUserIdAsNoTrackingAsync(UserId userId,
        CancellationToken cancellationToken);

    Task<IEnumerable<PaymentTransaction>> FindPaidByUserIdAsNoTrackingAsync(UserId userId,
        CancellationToken cancellationToken);
}

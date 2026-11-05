using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Billing.Domain.Model.Queries;

namespace Trazza.Platform.Billing.Application.QueryServices;

public interface IPaymentTransactionQueryService
{
    Task<IEnumerable<PaymentTransaction>> Handle(GetAllPaymentTransactionsQuery query,
        CancellationToken cancellationToken);

    Task<IEnumerable<PaymentTransaction>> Handle(GetPaymentTransactionsByUserIdQuery query,
        CancellationToken cancellationToken);

    Task<PaymentTransaction?> Handle(GetPaymentTransactionByIdQuery query, CancellationToken cancellationToken);

    Task<PaymentTransaction?> Handle(GetActiveSubscriptionByUserIdQuery query, CancellationToken cancellationToken);
}

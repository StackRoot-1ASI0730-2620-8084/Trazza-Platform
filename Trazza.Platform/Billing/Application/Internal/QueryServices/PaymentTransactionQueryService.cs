using Trazza.Platform.Billing.Application.QueryServices;
using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Billing.Domain.Model.Queries;
using Trazza.Platform.Billing.Domain.Repositories;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Billing.Application.Internal.QueryServices;

public class PaymentTransactionQueryService(IPaymentTransactionRepository paymentTransactionRepository)
    : IPaymentTransactionQueryService
{
    public async Task<IEnumerable<PaymentTransaction>> Handle(GetAllPaymentTransactionsQuery query,
        CancellationToken cancellationToken)
    {
        return await paymentTransactionRepository.ListAsNoTrackingAsync(cancellationToken);
    }

    public async Task<IEnumerable<PaymentTransaction>> Handle(GetPaymentTransactionsByUserIdQuery query,
        CancellationToken cancellationToken)
    {
        return await paymentTransactionRepository.FindByUserIdAsNoTrackingAsync(query.UserId, cancellationToken);
    }

    public async Task<PaymentTransaction?> Handle(GetPaymentTransactionByIdQuery query,
        CancellationToken cancellationToken)
    {
        return await paymentTransactionRepository.FindByIdAsNoTrackingAsync(query.PaymentTransactionId,
            cancellationToken);
    }

    public async Task<PaymentTransaction?> Handle(GetActiveSubscriptionByUserIdQuery query,
        CancellationToken cancellationToken)
    {
        var paidTransactions =
            await paymentTransactionRepository.FindPaidByUserIdAsNoTrackingAsync(query.UserId, cancellationToken);
        var today = LimaCalendar.Today;
        return paidTransactions
            .Where(transaction => transaction.Covers(today))
            .OrderByDescending(transaction => transaction.PeriodEnd)
            .FirstOrDefault();
    }
}

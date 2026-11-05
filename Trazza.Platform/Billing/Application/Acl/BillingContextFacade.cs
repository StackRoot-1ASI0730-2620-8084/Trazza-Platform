using Trazza.Platform.Billing.Application.QueryServices;
using Trazza.Platform.Billing.Domain.Model.Queries;
using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Billing.Interfaces.Acl;

namespace Trazza.Platform.Billing.Application.Acl;

public class BillingContextFacade(IPaymentTransactionQueryService paymentTransactionQueryService)
    : IBillingContextFacade
{
    public async Task<bool> CanPublish(int userId, int usedThisMonth, CancellationToken cancellationToken)
    {
        var plan = await FetchCurrentPlan(userId, cancellationToken);
        return plan.AllowsPublication(usedThisMonth);
    }

    public async Task<string> FetchCurrentPlanCode(int userId, CancellationToken cancellationToken)
    {
        var plan = await FetchCurrentPlan(userId, cancellationToken);
        return plan.Code;
    }

    private async Task<SubscriptionPlan> FetchCurrentPlan(int userId, CancellationToken cancellationToken)
    {
        var activeSubscription = await paymentTransactionQueryService.Handle(
            new GetActiveSubscriptionByUserIdQuery(new UserId(userId)), cancellationToken);
        return activeSubscription?.Plan ?? new SubscriptionPlan(SubscriptionPlan.Free);
    }
}

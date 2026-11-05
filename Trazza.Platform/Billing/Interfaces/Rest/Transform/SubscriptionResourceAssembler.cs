using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Billing.Interfaces.Rest.Resources;
using Trazza.Platform.Shared.Interfaces.Rest.Transform;

namespace Trazza.Platform.Billing.Interfaces.Rest.Transform;

public static class SubscriptionResourceAssembler
{
    public static SubscriptionPlanResource ToResourceFromPlan(SubscriptionPlan plan)
    {
        return new SubscriptionPlanResource(plan.Code, MoneyResourceAssembler.ToResourceFromValueObject(plan.Price),
            plan.MonthlyPublications);
    }

    public static UserSubscriptionResource ToResourceFromActiveSubscription(int userId,
        PaymentTransaction? activeSubscription)
    {
        var plan = activeSubscription?.Plan ?? new SubscriptionPlan(SubscriptionPlan.Free);
        return new UserSubscriptionResource(userId, plan.Code,
            MoneyResourceAssembler.ToResourceFromValueObject(plan.Price), plan.MonthlyPublications,
            activeSubscription?.PeriodStart, activeSubscription?.PeriodEnd);
    }
}

using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Billing.Interfaces.Rest.Resources;
using Trazza.Platform.Shared.Domain.Model.Extensions;
using Trazza.Platform.Shared.Interfaces.Rest.Transform;

namespace Trazza.Platform.Billing.Interfaces.Rest.Transform;

public static class PaymentTransactionResourceFromEntityAssembler
{
    public static PaymentTransactionResource ToResourceFromEntity(PaymentTransaction transaction)
    {
        return new PaymentTransactionResource(
            transaction.Id,
            transaction.UserId,
            transaction.Plan.Code,
            MoneyResourceAssembler.ToResourceFromValueObject(transaction.Amount),
            ToResourceFromPaymentMethod(transaction.Method),
            transaction.Status.ToSnakeCaseValue(),
            transaction.AuthorizationCode,
            transaction.CreatedAt,
            transaction.PaidAt,
            transaction.PeriodStart,
            transaction.PeriodEnd);
    }

    private static PaymentMethodResource ToResourceFromPaymentMethod(PaymentMethod method)
    {
        return new PaymentMethodResource(method.Brand, method.Last4, method.HolderName, method.Expiry, method.Label);
    }
}

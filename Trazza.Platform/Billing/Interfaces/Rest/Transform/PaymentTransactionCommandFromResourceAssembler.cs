using Trazza.Platform.Billing.Domain.Model.Commands;
using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Billing.Interfaces.Rest.Resources;

namespace Trazza.Platform.Billing.Interfaces.Rest.Transform;

public static class PaymentTransactionCommandFromResourceAssembler
{
    public static CreatePaymentTransactionCommand ToCommandFromResource(CreatePaymentTransactionResource resource)
    {
        var method = resource.Method is null
            ? new PaymentMethod()
            : string.IsNullOrWhiteSpace(resource.Method.CardNumber)
                ? PaymentMethod.Create(resource.Method.Brand, resource.Method.Last4, resource.Method.HolderName,
                    resource.Method.Expiry)
                : PaymentMethod.FromCard(resource.Method.CardNumber, resource.Method.HolderName,
                    resource.Method.Expiry);
        return new CreatePaymentTransactionCommand(new UserId(resource.UserId), resource.Plan, method);
    }
}

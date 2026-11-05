using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Billing.Interfaces.Rest.Resources;
using Trazza.Platform.Shared.Interfaces.Rest.Transform;

namespace Trazza.Platform.Billing.Interfaces.Rest.Transform;

public static class ReceiptResourceFromEntityAssembler
{
    public static ReceiptResource ToResourceFromEntity(Receipt receipt)
    {
        return new ReceiptResource(
            receipt.Id,
            receipt.TransactionId,
            receipt.UserId,
            receipt.Type.Value,
            receipt.Series,
            receipt.Number,
            receipt.Code,
            receipt.CustomerName,
            receipt.CustomerDocument,
            MoneyResourceAssembler.ToResourceFromValueObject(receipt.Total),
            MoneyResourceAssembler.ToResourceFromValueObject(receipt.Subtotal),
            MoneyResourceAssembler.ToResourceFromValueObject(receipt.Igv),
            receipt.IssuedAt);
    }
}

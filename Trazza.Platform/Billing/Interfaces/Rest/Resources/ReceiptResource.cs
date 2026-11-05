using Trazza.Platform.Shared.Interfaces.Rest.Resources;

namespace Trazza.Platform.Billing.Interfaces.Rest.Resources;

public record ReceiptResource(
    int Id,
    int TransactionId,
    int UserId,
    string Type,
    string Series,
    int Number,
    string Code,
    string CustomerName,
    string CustomerDocument,
    MoneyResource Total,
    MoneyResource Subtotal,
    MoneyResource Igv,
    DateTimeOffset IssuedAt);

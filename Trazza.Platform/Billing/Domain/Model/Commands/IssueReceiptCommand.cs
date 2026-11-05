using Trazza.Platform.Billing.Domain.Model.ValueObjects;

namespace Trazza.Platform.Billing.Domain.Model.Commands;

public record IssueReceiptCommand(
    PaymentTransactionId TransactionId,
    string Type,
    string CustomerName,
    string CustomerDocument);

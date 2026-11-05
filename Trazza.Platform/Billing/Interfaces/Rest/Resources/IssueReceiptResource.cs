namespace Trazza.Platform.Billing.Interfaces.Rest.Resources;

public record IssueReceiptResource(int TransactionId, string Type, string CustomerName, string CustomerDocument);

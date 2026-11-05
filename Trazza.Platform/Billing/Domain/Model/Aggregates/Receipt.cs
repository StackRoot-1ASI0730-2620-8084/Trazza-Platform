using Trazza.Platform.Billing.Domain.Model.Events;
using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.Aggregates.Internal;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Billing.Domain.Model.Aggregates;

public partial class Receipt : AggregateRoot
{
    public const decimal IgvRate = 0.18m;
    public const int MinCustomerNameLength = 3;

    public Receipt()
    {
        Type = new ReceiptType();
        CustomerName = string.Empty;
        CustomerDocument = string.Empty;
        Total = new Money();
    }

    public Receipt(PaymentTransaction transaction, ReceiptType type, int number, string customerName,
        string customerDocument, DateTimeOffset issuedAt) : this()
    {
        TransactionId = transaction.Id;
        UserId = transaction.UserId;
        Type = type;
        Number = number;
        CustomerName = customerName.Trim();
        CustomerDocument = customerDocument.Trim();
        Total = transaction.Amount;
        IssuedAt = issuedAt;
        AddDomainEvent(new ReceiptIssuedEvent(UserId, TransactionId, Type.Value));
    }

    public ReceiptId Id { get; }
    public PaymentTransactionId TransactionId { get; private set; }
    public UserId UserId { get; private set; }
    public ReceiptType Type { get; private set; }
    public int Number { get; private set; }
    public string CustomerName { get; private set; }
    public string CustomerDocument { get; private set; }
    public Money Total { get; private set; }
    public DateTimeOffset IssuedAt { get; private set; }

    public string Series => Type.Series;

    public string Code => $"{Series}-{Number:D8}";

    public Money Subtotal =>
        new(Math.Round(Total.Amount / (1 + IgvRate), 2, MidpointRounding.AwayFromZero), Total.Currency);

    public Money Igv => new(Total.Amount - Subtotal.Amount, Total.Currency);
}

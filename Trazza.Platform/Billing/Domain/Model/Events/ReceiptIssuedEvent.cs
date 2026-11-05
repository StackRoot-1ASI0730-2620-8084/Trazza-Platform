using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.Events;

namespace Trazza.Platform.Billing.Domain.Model.Events;

public record ReceiptIssuedEvent(UserId UserId, PaymentTransactionId TransactionId, string Type) : IEvent;

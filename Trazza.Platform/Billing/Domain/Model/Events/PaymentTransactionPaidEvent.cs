using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.Events;

namespace Trazza.Platform.Billing.Domain.Model.Events;

public record PaymentTransactionPaidEvent(UserId UserId, string Plan, DateOnly PeriodEnd) : IEvent;

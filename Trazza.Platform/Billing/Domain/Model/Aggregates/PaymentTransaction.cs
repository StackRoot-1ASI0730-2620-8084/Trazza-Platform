using Trazza.Platform.Billing.Domain.Model.Errors;
using Trazza.Platform.Billing.Domain.Model.Events;
using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.Aggregates.Internal;
using Trazza.Platform.Shared.Domain.Model.Errors;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Billing.Domain.Model.Aggregates;

public partial class PaymentTransaction : AggregateRoot
{
    public PaymentTransaction()
    {
        Plan = new SubscriptionPlan();
        Amount = new Money();
        Method = new PaymentMethod();
    }

    public PaymentTransaction(UserId userId, SubscriptionPlan plan, PaymentMethod method) : this()
    {
        UserId = userId;
        Plan = plan;
        Amount = plan.Price;
        Method = method;
        Status = EPaymentStatus.Pending;
    }

    public PaymentTransactionId Id { get; }
    public UserId UserId { get; private set; }
    public SubscriptionPlan Plan { get; private set; }
    public Money Amount { get; private set; }
    public PaymentMethod Method { get; private set; }
    public EPaymentStatus Status { get; private set; }
    public string? AuthorizationCode { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }
    public DateOnly? PeriodStart { get; private set; }
    public DateOnly? PeriodEnd { get; private set; }

    public bool IsPaid => Status == EPaymentStatus.Paid;

    public bool IsPending => Status == EPaymentStatus.Pending;

    public Error MarkPaid(string authorizationCode, DateTimeOffset now)
    {
        if (!IsPending) return BillingErrors.TransactionNotPending;
        if (string.IsNullOrWhiteSpace(authorizationCode)) return BillingErrors.AuthorizationRequired;

        var start = DateOnly.FromDateTime(now.ToOffset(LimaCalendar.UtcOffset).DateTime);
        Status = EPaymentStatus.Paid;
        AuthorizationCode = authorizationCode;
        PaidAt = now;
        PeriodStart = start;
        PeriodEnd = start.AddMonths(1).AddDays(-1);
        AddDomainEvent(new PaymentTransactionPaidEvent(UserId, Plan.Code, PeriodEnd.Value));
        return Error.None;
    }

    public Error MarkFailed()
    {
        if (!IsPending) return BillingErrors.TransactionNotPending;

        Status = EPaymentStatus.Failed;
        return Error.None;
    }

    public bool Covers(DateOnly today) =>
        IsPaid && PeriodStart <= today && today <= PeriodEnd;
}

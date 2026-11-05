using Trazza.Platform.Shared.Interfaces.Rest.Resources;

namespace Trazza.Platform.Billing.Interfaces.Rest.Resources;

public record PaymentTransactionResource(
    int Id,
    int UserId,
    string Plan,
    MoneyResource Amount,
    PaymentMethodResource Method,
    string Status,
    string? AuthorizationCode,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? PaidAt,
    DateOnly? PeriodStart,
    DateOnly? PeriodEnd);

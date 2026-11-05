using Trazza.Platform.Shared.Interfaces.Rest.Resources;

namespace Trazza.Platform.Billing.Interfaces.Rest.Resources;

public record UserSubscriptionResource(
    int UserId,
    string Plan,
    MoneyResource Price,
    int? MonthlyPublications,
    DateOnly? PeriodStart,
    DateOnly? PeriodEnd);

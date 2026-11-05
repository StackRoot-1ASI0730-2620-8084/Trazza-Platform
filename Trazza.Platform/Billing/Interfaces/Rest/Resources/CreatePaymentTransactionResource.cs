namespace Trazza.Platform.Billing.Interfaces.Rest.Resources;

public record CreatePaymentTransactionResource(int UserId, string Plan, SavePaymentMethodResource Method);

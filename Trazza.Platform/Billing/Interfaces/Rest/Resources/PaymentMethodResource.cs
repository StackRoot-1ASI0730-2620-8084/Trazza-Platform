namespace Trazza.Platform.Billing.Interfaces.Rest.Resources;

public record PaymentMethodResource(string Brand, string Last4, string HolderName, string Expiry, string Label);

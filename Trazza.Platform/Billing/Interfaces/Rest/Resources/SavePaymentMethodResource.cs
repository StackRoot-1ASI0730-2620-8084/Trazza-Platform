namespace Trazza.Platform.Billing.Interfaces.Rest.Resources;

public record SavePaymentMethodResource(string? Brand, string? Last4, string? HolderName, string? Expiry, string? CardNumber);

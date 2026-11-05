namespace Trazza.Platform.Billing.Application.Internal.OutboundServices;

public record PaymentGatewayResponse(bool Approved, string? AuthorizationCode);

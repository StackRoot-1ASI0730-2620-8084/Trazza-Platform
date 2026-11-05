using Trazza.Platform.Billing.Application.Internal.OutboundServices;
using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Billing.Infrastructure.Payments.Simulated.Services;

public class SimulatedPaymentGateway : IPaymentGateway
{
    public const string DeclinedTestCardLast4 = "0002";

    public Task<PaymentGatewayResponse> Charge(Money amount, PaymentMethod method, CancellationToken cancellationToken)
    {
        var approved = method.Last4 != DeclinedTestCardLast4 && amount.Amount > 0;
        var authorizationCode = approved
            ? $"AUT-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds():X}"
            : null;
        return Task.FromResult(new PaymentGatewayResponse(approved, authorizationCode));
    }
}

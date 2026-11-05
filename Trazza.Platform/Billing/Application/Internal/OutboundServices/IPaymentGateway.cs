using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Billing.Application.Internal.OutboundServices;

public interface IPaymentGateway
{
    Task<PaymentGatewayResponse> Charge(Money amount, PaymentMethod method, CancellationToken cancellationToken);
}

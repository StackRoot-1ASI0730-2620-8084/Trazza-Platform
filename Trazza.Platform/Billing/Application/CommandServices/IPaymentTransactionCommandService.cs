using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Billing.Domain.Model.Commands;
using Trazza.Platform.Shared.Application.Model;

namespace Trazza.Platform.Billing.Application.CommandServices;

public interface IPaymentTransactionCommandService
{
    Task<Result<PaymentTransaction>> Handle(CreatePaymentTransactionCommand command,
        CancellationToken cancellationToken);
}

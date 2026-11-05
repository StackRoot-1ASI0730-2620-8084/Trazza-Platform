using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Billing.Domain.Model.Commands;
using Trazza.Platform.Shared.Application.Model;

namespace Trazza.Platform.Billing.Application.CommandServices;

public interface IReceiptCommandService
{
    Task<Result<Receipt>> Handle(IssueReceiptCommand command, CancellationToken cancellationToken);
}

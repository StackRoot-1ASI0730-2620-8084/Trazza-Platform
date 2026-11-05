using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Billing.Domain.Model.Queries;

namespace Trazza.Platform.Billing.Application.QueryServices;

public interface IReceiptQueryService
{
    Task<IEnumerable<Receipt>> Handle(GetAllReceiptsQuery query, CancellationToken cancellationToken);

    Task<IEnumerable<Receipt>> Handle(GetReceiptsByUserIdQuery query, CancellationToken cancellationToken);

    Task<Receipt?> Handle(GetReceiptByIdQuery query, CancellationToken cancellationToken);
}

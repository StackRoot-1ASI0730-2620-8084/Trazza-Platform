using Trazza.Platform.Billing.Application.QueryServices;
using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Billing.Domain.Model.Queries;
using Trazza.Platform.Billing.Domain.Repositories;

namespace Trazza.Platform.Billing.Application.Internal.QueryServices;

public class ReceiptQueryService(IReceiptRepository receiptRepository) : IReceiptQueryService
{
    public async Task<IEnumerable<Receipt>> Handle(GetAllReceiptsQuery query, CancellationToken cancellationToken)
    {
        return await receiptRepository.ListAsNoTrackingAsync(cancellationToken);
    }

    public async Task<IEnumerable<Receipt>> Handle(GetReceiptsByUserIdQuery query, CancellationToken cancellationToken)
    {
        return await receiptRepository.FindByUserIdAsNoTrackingAsync(query.UserId, cancellationToken);
    }

    public async Task<Receipt?> Handle(GetReceiptByIdQuery query, CancellationToken cancellationToken)
    {
        return await receiptRepository.FindByIdAsNoTrackingAsync(query.ReceiptId, cancellationToken);
    }
}

using Trazza.Platform.Billing.Domain.Model.Commands;
using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Billing.Interfaces.Rest.Resources;

namespace Trazza.Platform.Billing.Interfaces.Rest.Transform;

public static class IssueReceiptCommandFromResourceAssembler
{
    public static IssueReceiptCommand ToCommandFromResource(IssueReceiptResource resource)
    {
        return new IssueReceiptCommand(new PaymentTransactionId(resource.TransactionId), resource.Type,
            resource.CustomerName, resource.CustomerDocument);
    }
}

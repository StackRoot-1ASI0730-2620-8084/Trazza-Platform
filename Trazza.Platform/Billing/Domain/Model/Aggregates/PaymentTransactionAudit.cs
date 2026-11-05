using Trazza.Platform.Shared.Domain.Model.Entities;

namespace Trazza.Platform.Billing.Domain.Model.Aggregates;

public partial class PaymentTransaction : IAuditableEntity
{
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

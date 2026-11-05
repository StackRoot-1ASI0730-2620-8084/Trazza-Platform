using Trazza.Platform.Shared.Domain.Model.Entities;

namespace Trazza.Platform.Reputation.Domain.Model.Aggregates;

public partial class Rating : IAuditableEntity
{
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
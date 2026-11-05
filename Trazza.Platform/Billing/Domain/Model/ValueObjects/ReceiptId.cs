using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Billing.Domain.Model.ValueObjects;

public readonly record struct ReceiptId(int Value) : IEntityId<int>
{
    public static implicit operator int(ReceiptId id) => id.Value;
    public static implicit operator ReceiptId(int value) => new(value);

    public override string ToString() => Value.ToString();
}

using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Billing.Domain.Model.ValueObjects;

public readonly record struct PaymentTransactionId(int Value) : IEntityId<int>
{
    public static implicit operator int(PaymentTransactionId id) => id.Value;
    public static implicit operator PaymentTransactionId(int value) => new(value);

    public override string ToString() => Value.ToString();
}

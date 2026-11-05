using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Reputation.Domain.Model.ValueObjects;

public readonly record struct ShipmentId(int Value) : IEntityId<int>
{
    public static implicit operator int(ShipmentId id) => id.Value;
    public static implicit operator ShipmentId(int value) => new(value);

    public override string ToString() => Value.ToString();
}
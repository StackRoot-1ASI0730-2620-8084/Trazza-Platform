using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Reputation.Domain.Model.ValueObjects;

public readonly record struct RatedUserId(int Value) : IEntityId<int>
{
    public static implicit operator int(RatedUserId id) => id.Value;
    public static implicit operator RatedUserId(int value) => new(value);

    public override string ToString() => Value.ToString();
}
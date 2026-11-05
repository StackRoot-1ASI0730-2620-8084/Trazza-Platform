using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Reputation.Domain.Model.ValueObjects;

public readonly record struct RaterId(int Value) : IEntityId<int>
{
    public static implicit operator int(RaterId id) => id.Value;
    public static implicit operator RaterId(int value) => new(value);

    public override string ToString() => Value.ToString();
}
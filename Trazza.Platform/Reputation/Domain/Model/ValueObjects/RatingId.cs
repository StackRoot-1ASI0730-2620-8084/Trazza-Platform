using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Reputation.Domain.Model.ValueObjects;

public readonly record struct RatingId(int Value) : IEntityId<int>
{
    public static implicit operator int(RatingId id) => id.Value;
    public static implicit operator RatingId(int value) => new(value);

    public override string ToString() => Value.ToString();
}
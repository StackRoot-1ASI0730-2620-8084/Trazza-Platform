namespace Trazza.Platform.Reputation.Domain.Model.ValueObjects;

public record Score(int Value)
{
    public const int Min = 1;
    public const int Max = 5;
    public const int PositiveThreshold = 4;

    public Score() : this(0)
    {
    }

    public bool IsValid => Value is >= Min and <= Max;

    public bool IsPositive => Value >= PositiveThreshold;
}
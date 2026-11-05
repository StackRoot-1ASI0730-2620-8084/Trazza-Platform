namespace Trazza.Platform.Reputation.Domain.Model.ValueObjects;

public record RatingTarget(RatedUserId UserId, string Role, string Name)
{
    public const string Carrier = "carrier";
    public const string Merchant = "merchant";

    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> TagsByRole =
        new Dictionary<string, IReadOnlyList<string>>
        {
            [Carrier] = ["on_time", "careful_with_goods", "friendly", "good_communication"],
            [Merchant] = ["on_time_pickup", "accurate_load_info", "friendly", "good_communication"]
        };

    public RatingTarget() : this(new RatedUserId(), string.Empty, string.Empty)
    {
    }

    public bool IsValid => TagsByRole.ContainsKey(Role);

    public IReadOnlyList<string> AllowedTags => TagsByRole.GetValueOrDefault(Role) ?? [];

    public bool AllowsTag(string tag) => AllowedTags.Contains(tag);
}
using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Billing.Domain.Model.ValueObjects;

public record SubscriptionPlan(string Code)
{
    public const string Free = "free";
    public const string Pro = "pro";

    private static readonly IReadOnlyDictionary<string, (decimal Price, int? MonthlyPublications)> Catalog =
        new Dictionary<string, (decimal Price, int? MonthlyPublications)>
        {
            [Free] = (0m, 5),
            [Pro] = (39.90m, null)
        };

    public SubscriptionPlan() : this(string.Empty)
    {
    }

    public static IReadOnlyList<SubscriptionPlan> All => Catalog.Keys.Select(code => new SubscriptionPlan(code)).ToList();

    public static SubscriptionPlan From(string? code) => new((code ?? string.Empty).Trim().ToLowerInvariant());

    public bool IsValid => Catalog.ContainsKey(Code);

    public bool IsFree => Code == Free;

    public Money Price => new(IsValid ? Catalog[Code].Price : 0m);

    public int? MonthlyPublications => IsValid ? Catalog[Code].MonthlyPublications : 0;

    public bool AllowsPublication(int usedThisMonth) =>
        MonthlyPublications is null || usedThisMonth < MonthlyPublications;
}

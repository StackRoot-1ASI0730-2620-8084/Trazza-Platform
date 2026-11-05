using System.Text.RegularExpressions;

namespace Trazza.Platform.Billing.Domain.Model.ValueObjects;

public partial record PaymentMethod(string Brand, string Last4, string HolderName, string Expiry)
{
    public const int MinHolderNameLength = 3;

    public static readonly IReadOnlyList<string> Brands = ["visa", "mastercard", "amex"];

    public PaymentMethod() : this(string.Empty, string.Empty, string.Empty, string.Empty)
    {
    }

    public bool IsValid =>
        Brands.Contains(Brand) &&
        Last4Pattern().IsMatch(Last4) &&
        HolderName.Trim().Length >= MinHolderNameLength &&
        ExpiryPattern().IsMatch(Expiry);

    public string Label => $"{Brand.ToUpperInvariant()} •••• {Last4}";

    public static PaymentMethod Create(string? brand, string? last4, string? holderName, string? expiry) =>
        new((brand ?? string.Empty).Trim().ToLowerInvariant(), (last4 ?? string.Empty).Trim(),
            (holderName ?? string.Empty).Trim().ToUpperInvariant(), (expiry ?? string.Empty).Trim());

    public static PaymentMethod FromCard(string? number, string? holderName, string? expiry)
    {
        var digits = NonDigit().Replace(number ?? string.Empty, string.Empty);
        if (digits.Length is < 13 or > 19 || !PassesLuhn(digits)) return new PaymentMethod();

        var brand = DetectBrand(digits);
        return brand is null ? new PaymentMethod() : Create(brand, digits[^4..], holderName, expiry);
    }

    public bool IsExpired(DateOnly today)
    {
        if (!ExpiryPattern().IsMatch(Expiry)) return true;

        var month = int.Parse(Expiry[..2]);
        var year = 2000 + int.Parse(Expiry[3..]);
        var lastValidDay = new DateOnly(year, month, DateTime.DaysInMonth(year, month));
        return lastValidDay < today;
    }

    public static bool PassesLuhn(string digits)
    {
        var sum = 0;
        var doubleDigit = false;
        for (var index = digits.Length - 1; index >= 0; index--)
        {
            var digit = digits[index] - '0';
            if (doubleDigit)
            {
                digit *= 2;
                if (digit > 9) digit -= 9;
            }

            sum += digit;
            doubleDigit = !doubleDigit;
        }

        return sum % 10 == 0;
    }

    public static string? DetectBrand(string digits)
    {
        if (VisaPattern().IsMatch(digits)) return "visa";
        if (MastercardPattern().IsMatch(digits)) return "mastercard";
        if (AmexPattern().IsMatch(digits)) return "amex";
        return null;
    }

    [GeneratedRegex(@"^\d{4}$")]
    private static partial Regex Last4Pattern();

    [GeneratedRegex(@"^(0[1-9]|1[0-2])/\d{2}$")]
    private static partial Regex ExpiryPattern();

    [GeneratedRegex(@"\D")]
    private static partial Regex NonDigit();

    [GeneratedRegex("^4")]
    private static partial Regex VisaPattern();

    [GeneratedRegex("^(5[1-5]|2[2-7])")]
    private static partial Regex MastercardPattern();

    [GeneratedRegex("^3[47]")]
    private static partial Regex AmexPattern();
}

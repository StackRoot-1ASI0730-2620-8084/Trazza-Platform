using System.Text.RegularExpressions;

namespace Trazza.Platform.Billing.Domain.Model.ValueObjects;

public partial record ReceiptType(string Value)
{
    public const string Boleta = "boleta";
    public const string Factura = "factura";

    public ReceiptType() : this(string.Empty)
    {
    }

    public static ReceiptType From(string? value) => new((value ?? string.Empty).Trim().ToLowerInvariant());

    public bool IsValid => Value is Boleta or Factura;

    public bool IsFactura => Value == Factura;

    public string Series => IsFactura ? "F001" : "B001";

    public bool AcceptsDocument(string? documentNumber) =>
        IsFactura
            ? RucPattern().IsMatch(documentNumber ?? string.Empty)
            : DniPattern().IsMatch(documentNumber ?? string.Empty);

    [GeneratedRegex(@"^\d{8}$")]
    private static partial Regex DniPattern();

    [GeneratedRegex(@"^(10|20)\d{9}$")]
    private static partial Regex RucPattern();
}

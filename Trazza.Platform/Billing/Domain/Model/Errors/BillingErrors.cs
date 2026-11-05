using Trazza.Platform.Shared.Domain.Model.Errors;

namespace Trazza.Platform.Billing.Domain.Model.Errors;

public static class BillingErrors
{
    public static readonly Error PaymentTransactionNotFound =
        Error.NotFound("Billing.PaymentTransactionNotFound", "The specified payment transaction was not found.");

    public static readonly Error ReceiptNotFound =
        Error.NotFound("Billing.ReceiptNotFound", "The specified receipt was not found.");

    public static readonly Error UserNotFound =
        Error.NotFound("Billing.UserNotFound", "The paying user was not found.");

    public static readonly Error InvalidPlan =
        Error.Validation("Billing.InvalidPlan", "The subscription plan is not supported.");

    public static readonly Error PlanFreeNotChargeable =
        Error.Validation("Billing.PlanFreeNotChargeable", "The Free plan cannot be charged.");

    public static readonly Error PlanAlreadyActive =
        Error.Conflict("Billing.PlanAlreadyActive", "The user already has an active paid subscription.");

    public static readonly Error InvalidPaymentMethod =
        Error.Validation("Billing.InvalidPaymentMethod", "The card data is not valid.");

    public static readonly Error CardExpired =
        Error.Validation("Billing.CardExpired", "The card is expired.");

    public static readonly Error TransactionNotPending =
        Error.Conflict("Billing.TransactionNotPending", "The payment transaction is not pending.");

    public static readonly Error AuthorizationRequired =
        Error.Validation("Billing.AuthorizationRequired", "An authorization code is required to mark the payment as paid.");

    public static readonly Error TransactionNotPaid =
        Error.Conflict("Billing.TransactionNotPaid", "A receipt can only be issued for a paid transaction.");

    public static readonly Error ReceiptAlreadyIssued =
        Error.Conflict("Billing.ReceiptAlreadyIssued", "A receipt was already issued for this transaction.");

    public static readonly Error InvalidReceiptType =
        Error.Validation("Billing.InvalidReceiptType", "The receipt type must be boleta or factura.");

    public static readonly Error InvalidCustomerName =
        Error.Validation("Billing.InvalidCustomerName", "The customer name must have at least 3 characters.");

    public static readonly Error InvalidCustomerDni =
        Error.Validation("Billing.InvalidCustomerDni", "A boleta requires a DNI of 8 digits.");

    public static readonly Error InvalidCustomerRuc =
        Error.Validation("Billing.InvalidCustomerRuc", "A factura requires a RUC of 11 digits starting with 10 or 20.");
}

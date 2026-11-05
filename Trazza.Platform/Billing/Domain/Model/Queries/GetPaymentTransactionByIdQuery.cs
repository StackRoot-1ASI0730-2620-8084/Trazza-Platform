using Trazza.Platform.Billing.Domain.Model.ValueObjects;

namespace Trazza.Platform.Billing.Domain.Model.Queries;

public record GetPaymentTransactionByIdQuery(PaymentTransactionId PaymentTransactionId);

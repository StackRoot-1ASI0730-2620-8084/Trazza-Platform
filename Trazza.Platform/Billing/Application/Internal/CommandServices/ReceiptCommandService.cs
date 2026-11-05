using Trazza.Platform.Billing.Application.CommandServices;
using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Billing.Domain.Model.Commands;
using Trazza.Platform.Billing.Domain.Model.Errors;
using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Billing.Domain.Repositories;
using Trazza.Platform.Shared.Application.Model;
using Trazza.Platform.Shared.Domain.Repositories;

namespace Trazza.Platform.Billing.Application.Internal.CommandServices;

public class ReceiptCommandService(
    IReceiptRepository receiptRepository,
    IPaymentTransactionRepository paymentTransactionRepository,
    IUnitOfWork unitOfWork)
    : IReceiptCommandService
{
    public async Task<Result<Receipt>> Handle(IssueReceiptCommand command, CancellationToken cancellationToken)
    {
        var type = ReceiptType.From(command.Type);
        var customerName = (command.CustomerName ?? string.Empty).Trim();
        var customerDocument = (command.CustomerDocument ?? string.Empty).Trim();

        if (!type.IsValid)
            return Result<Receipt>.Failure(BillingErrors.InvalidReceiptType);
        if (customerName.Length < Receipt.MinCustomerNameLength)
            return Result<Receipt>.Failure(BillingErrors.InvalidCustomerName);
        if (!type.AcceptsDocument(customerDocument))
            return Result<Receipt>.Failure(type.IsFactura
                ? BillingErrors.InvalidCustomerRuc
                : BillingErrors.InvalidCustomerDni);

        var transaction = await paymentTransactionRepository.FindByIdAsync(command.TransactionId, cancellationToken);
        if (transaction is null)
            return Result<Receipt>.Failure(BillingErrors.PaymentTransactionNotFound);
        if (!transaction.IsPaid)
            return Result<Receipt>.Failure(BillingErrors.TransactionNotPaid);
        if (await receiptRepository.ExistsByTransactionIdAsync(transaction.Id, cancellationToken))
            return Result<Receipt>.Failure(BillingErrors.ReceiptAlreadyIssued);

        var number = await receiptRepository.FindLastNumberByTypeAsync(type, cancellationToken) + 1;
        var receipt = new Receipt(transaction, type, number, customerName, customerDocument, DateTimeOffset.UtcNow);

        await receiptRepository.AddAsync(receipt, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<Receipt>.Success(receipt);
    }
}

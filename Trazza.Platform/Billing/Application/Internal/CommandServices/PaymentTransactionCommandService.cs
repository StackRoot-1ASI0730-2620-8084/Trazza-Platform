using Trazza.Platform.Billing.Application.CommandServices;
using Trazza.Platform.Billing.Application.Internal.OutboundServices;
using Trazza.Platform.Billing.Application.QueryServices;
using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Billing.Domain.Model.Commands;
using Trazza.Platform.Billing.Domain.Model.Errors;
using Trazza.Platform.Billing.Domain.Model.Queries;
using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Billing.Domain.Repositories;
using Trazza.Platform.Iam.Interfaces.Acl;
using Trazza.Platform.Shared.Application.Model;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Repositories;

namespace Trazza.Platform.Billing.Application.Internal.CommandServices;

public class PaymentTransactionCommandService(
    IPaymentTransactionRepository paymentTransactionRepository,
    IPaymentTransactionQueryService paymentTransactionQueryService,
    IPaymentGateway paymentGateway,
    IIamContextFacade iamContextFacade,
    IUnitOfWork unitOfWork)
    : IPaymentTransactionCommandService
{
    public async Task<Result<PaymentTransaction>> Handle(CreatePaymentTransactionCommand command,
        CancellationToken cancellationToken)
    {
        var plan = SubscriptionPlan.From(command.Plan);
        if (!plan.IsValid)
            return Result<PaymentTransaction>.Failure(BillingErrors.InvalidPlan);
        if (plan.IsFree)
            return Result<PaymentTransaction>.Failure(BillingErrors.PlanFreeNotChargeable);
        if (!command.Method.IsValid)
            return Result<PaymentTransaction>.Failure(BillingErrors.InvalidPaymentMethod);
        if (command.Method.IsExpired(LimaCalendar.Today))
            return Result<PaymentTransaction>.Failure(BillingErrors.CardExpired);
        if (!await iamContextFacade.ExistsUserById(command.UserId, cancellationToken))
            return Result<PaymentTransaction>.Failure(BillingErrors.UserNotFound);

        var activeSubscription = await paymentTransactionQueryService.Handle(
            new GetActiveSubscriptionByUserIdQuery(command.UserId), cancellationToken);
        if (activeSubscription is not null)
            return Result<PaymentTransaction>.Failure(BillingErrors.PlanAlreadyActive);

        var transaction = new PaymentTransaction(command.UserId, plan, command.Method);
        var response = await paymentGateway.Charge(transaction.Amount, transaction.Method, cancellationToken);
        if (response.Approved)
            transaction.MarkPaid(response.AuthorizationCode ?? string.Empty, DateTimeOffset.UtcNow);
        else
            transaction.MarkFailed();

        await paymentTransactionRepository.AddAsync(transaction, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<PaymentTransaction>.Success(transaction);
    }
}

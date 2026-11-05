using System.Net.Mime;
using Trazza.Platform.Billing.Application.CommandServices;
using Trazza.Platform.Billing.Application.QueryServices;
using Trazza.Platform.Billing.Domain.Model.Errors;
using Trazza.Platform.Billing.Domain.Model.Queries;
using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Billing.Interfaces.Rest.Resources;
using Trazza.Platform.Billing.Interfaces.Rest.Transform;
using Trazza.Platform.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using Trazza.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;

namespace Trazza.Platform.Billing.Interfaces.Rest;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Payment Transactions")]
public class PaymentTransactionsController(
    IPaymentTransactionCommandService paymentTransactionCommandService,
    IPaymentTransactionQueryService paymentTransactionQueryService,
    ProblemDetailsFactory problemDetailsFactory)
    : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Get payment transactions")]
    [EndpointDescription("Gets all payment transactions, optionally filtered by user id.")]
    [ProducesResponseType(typeof(IEnumerable<PaymentTransactionResource>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentTransactions([FromQuery] int? userId,
        CancellationToken cancellationToken)
    {
        var transactions = userId.HasValue
            ? await paymentTransactionQueryService.Handle(
                new GetPaymentTransactionsByUserIdQuery(new UserId(userId.Value)), cancellationToken)
            : await paymentTransactionQueryService.Handle(new GetAllPaymentTransactionsQuery(), cancellationToken);
        return Ok(transactions.Select(PaymentTransactionResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{paymentTransactionId:int}")]
    [EndpointSummary("Get payment transaction by id")]
    [EndpointDescription("Gets a payment transaction by its unique identifier.")]
    [ProducesResponseType(typeof(PaymentTransactionResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentTransactionById(int paymentTransactionId,
        CancellationToken cancellationToken)
    {
        var transaction = await paymentTransactionQueryService.Handle(
            new GetPaymentTransactionByIdQuery(new PaymentTransactionId(paymentTransactionId)), cancellationToken);

        return BillingActionResultAssembler.ToActionResultFromEntity(
            this,
            transaction,
            BillingErrors.PaymentTransactionNotFound,
            problemDetailsFactory,
            found => Ok(PaymentTransactionResourceFromEntityAssembler.ToResourceFromEntity(found)));
    }

    [HttpPost]
    [EndpointSummary("Upgrade plan")]
    [EndpointDescription("Charges the Pro plan to the card through the payment gateway and records the transaction as paid or failed.")]
    [ProducesResponseType(typeof(PaymentTransactionResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreatePaymentTransaction([FromBody] CreatePaymentTransactionResource resource,
        CancellationToken cancellationToken)
    {
        var command = PaymentTransactionCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await paymentTransactionCommandService.Handle(command, cancellationToken);

        return BillingActionResultAssembler.ToActionResultFromResult(
            this,
            result,
            problemDetailsFactory,
            created => CreatedAtAction(nameof(GetPaymentTransactionById),
                new { paymentTransactionId = created.Id.Value },
                PaymentTransactionResourceFromEntityAssembler.ToResourceFromEntity(created)));
    }
}

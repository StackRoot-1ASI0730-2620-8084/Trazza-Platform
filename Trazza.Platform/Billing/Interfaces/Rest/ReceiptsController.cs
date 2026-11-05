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
[Tags("Receipts")]
public class ReceiptsController(
    IReceiptCommandService receiptCommandService,
    IReceiptQueryService receiptQueryService,
    ProblemDetailsFactory problemDetailsFactory)
    : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Get receipts")]
    [EndpointDescription("Gets all electronic receipts, optionally filtered by user id.")]
    [ProducesResponseType(typeof(IEnumerable<ReceiptResource>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReceipts([FromQuery] int? userId, CancellationToken cancellationToken)
    {
        var receipts = userId.HasValue
            ? await receiptQueryService.Handle(new GetReceiptsByUserIdQuery(new UserId(userId.Value)),
                cancellationToken)
            : await receiptQueryService.Handle(new GetAllReceiptsQuery(), cancellationToken);
        return Ok(receipts.Select(ReceiptResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{receiptId:int}")]
    [EndpointSummary("Get receipt by id")]
    [EndpointDescription("Gets an electronic receipt by its unique identifier.")]
    [ProducesResponseType(typeof(ReceiptResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReceiptById(int receiptId, CancellationToken cancellationToken)
    {
        var receipt = await receiptQueryService.Handle(new GetReceiptByIdQuery(new ReceiptId(receiptId)),
            cancellationToken);

        return BillingActionResultAssembler.ToActionResultFromEntity(
            this,
            receipt,
            BillingErrors.ReceiptNotFound,
            problemDetailsFactory,
            found => Ok(ReceiptResourceFromEntityAssembler.ToResourceFromEntity(found)));
    }

    [HttpPost]
    [EndpointSummary("Issue receipt")]
    [EndpointDescription("Issues a boleta or factura for a paid transaction with the next correlative number.")]
    [ProducesResponseType(typeof(ReceiptResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> IssueReceipt([FromBody] IssueReceiptResource resource,
        CancellationToken cancellationToken)
    {
        var command = IssueReceiptCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await receiptCommandService.Handle(command, cancellationToken);

        return BillingActionResultAssembler.ToActionResultFromResult(
            this,
            result,
            problemDetailsFactory,
            issued => CreatedAtAction(nameof(GetReceiptById), new { receiptId = issued.Id.Value },
                ReceiptResourceFromEntityAssembler.ToResourceFromEntity(issued)));
    }
}

using System.Net.Mime;
using Trazza.Platform.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using Trazza.Platform.Reputation.Application.CommandServices;
using Trazza.Platform.Reputation.Application.QueryServices;
using Trazza.Platform.Reputation.Domain.Model.Aggregates;
using Trazza.Platform.Reputation.Domain.Model.Errors;
using Trazza.Platform.Reputation.Domain.Model.Queries;
using Trazza.Platform.Reputation.Domain.Model.ValueObjects;
using Trazza.Platform.Reputation.Interfaces.Rest.Resources;
using Trazza.Platform.Reputation.Interfaces.Rest.Transform;
using Trazza.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;

namespace Trazza.Platform.Reputation.Interfaces.Rest;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Ratings")]
public class RatingsController(
    IRatingCommandService ratingCommandService,
    IRatingQueryService ratingQueryService,
    ProblemDetailsFactory problemDetailsFactory)
    : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Get ratings")]
    [EndpointDescription("Gets all ratings, optionally filtered by rated user, rater or shipment.")]
    [ProducesResponseType(typeof(IEnumerable<RatingResource>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRatings([FromQuery] int? targetId, [FromQuery] int? raterId,
        [FromQuery] int? shipmentId, CancellationToken cancellationToken)
    {
        IEnumerable<Rating> ratings;
        if (targetId.HasValue)
            ratings = await ratingQueryService.Handle(
                new GetRatingsByRatedUserIdQuery(new RatedUserId(targetId.Value)), cancellationToken);
        else if (raterId.HasValue)
            ratings = await ratingQueryService.Handle(new GetRatingsByRaterIdQuery(new RaterId(raterId.Value)),
                cancellationToken);
        else if (shipmentId.HasValue)
            ratings = await ratingQueryService.Handle(
                new GetRatingsByShipmentIdQuery(new ShipmentId(shipmentId.Value)), cancellationToken);
        else
            ratings = await ratingQueryService.Handle(new GetAllRatingsQuery(), cancellationToken);

        return Ok(ratings.Select(RatingResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{ratingId:int}")]
    [EndpointSummary("Get rating by id")]
    [EndpointDescription("Gets a rating by its unique identifier.")]
    [ProducesResponseType(typeof(RatingResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRatingById(int ratingId, CancellationToken cancellationToken)
    {
        var rating = await ratingQueryService.Handle(new GetRatingByIdQuery(new RatingId(ratingId)),
            cancellationToken);

        return ReputationActionResultAssembler.ToActionResultFromEntity(
            this,
            rating,
            ReputationErrors.RatingNotFound,
            problemDetailsFactory,
            found => Ok(RatingResourceFromEntityAssembler.ToResourceFromEntity(found)));
    }

    [HttpPost]
    [EndpointSummary("Rate counterpart")]
    [EndpointDescription("Rates the counterpart of a delivered shipment once per rater.")]
    [ProducesResponseType(typeof(RatingResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SubmitRating([FromBody] SubmitRatingResource resource,
        CancellationToken cancellationToken)
    {
        var command = SubmitRatingCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await ratingCommandService.Handle(command, cancellationToken);

        return ReputationActionResultAssembler.ToActionResultFromResult(
            this,
            result,
            problemDetailsFactory,
            created => CreatedAtAction(nameof(GetRatingById), new { ratingId = created.Id.Value },
                RatingResourceFromEntityAssembler.ToResourceFromEntity(created)));
    }
}

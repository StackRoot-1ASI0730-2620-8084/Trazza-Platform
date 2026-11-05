using System.Net.Mime;
using Trazza.Platform.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using Trazza.Platform.Reputation.Application.QueryServices;
using Trazza.Platform.Reputation.Domain.Model.Queries;
using Trazza.Platform.Reputation.Domain.Model.ValueObjects;
using Trazza.Platform.Reputation.Interfaces.Rest.Resources;
using Trazza.Platform.Reputation.Interfaces.Rest.Transform;
using Microsoft.AspNetCore.Mvc;

namespace Trazza.Platform.Reputation.Interfaces.Rest;

[Authorize]
[ApiController]
[Route("api/v1/users/{userId:int}/reputation")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Ratings")]
public class UserReputationController(IRatingQueryService ratingQueryService) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Get user reputation")]
    [EndpointDescription("Gets the average score, total and positive ratings received by a user.")]
    [ProducesResponseType(typeof(ReputationSummaryResource), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserReputation(int userId, CancellationToken cancellationToken)
    {
        var summary = await ratingQueryService.Handle(new GetReputationSummaryQuery(new RatedUserId(userId)),
            cancellationToken);
        return Ok(ReputationSummaryResourceFromValueObjectAssembler.ToResourceFromValueObject(summary));
    }
}
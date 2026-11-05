using System.Net.Mime;
using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Billing.Interfaces.Rest.Resources;
using Trazza.Platform.Billing.Interfaces.Rest.Transform;
using Trazza.Platform.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Trazza.Platform.Billing.Interfaces.Rest;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Subscriptions")]
public class SubscriptionPlansController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [EndpointSummary("Get subscription plans")]
    [EndpointDescription("Gets the catalog of subscription plans with price and monthly publication limit.")]
    [ProducesResponseType(typeof(IEnumerable<SubscriptionPlanResource>), StatusCodes.Status200OK)]
    public IActionResult GetSubscriptionPlans()
    {
        return Ok(SubscriptionPlan.All.Select(SubscriptionResourceAssembler.ToResourceFromPlan));
    }
}

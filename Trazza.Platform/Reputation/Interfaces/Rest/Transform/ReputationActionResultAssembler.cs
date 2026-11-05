using Trazza.Platform.Shared.Application.Model;
using Trazza.Platform.Shared.Domain.Model.Errors;
using Trazza.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;

namespace Trazza.Platform.Reputation.Interfaces.Rest.Transform;

public static class ReputationActionResultAssembler
{
    public static IActionResult ToActionResultFromResult<T>(
        ControllerBase controller,
        Result<T> result,
        ProblemDetailsFactory problemDetailsFactory,
        Func<T, IActionResult> successAction)
    {
        return result.Match(
            successAction,
            error => problemDetailsFactory.CreateProblemDetails(controller, error));
    }

    public static IActionResult ToActionResultFromEntity<T>(
        ControllerBase controller,
        T? entity,
        Error notFoundError,
        ProblemDetailsFactory problemDetailsFactory,
        Func<T, IActionResult> successAction) where T : class
    {
        return entity is null
            ? problemDetailsFactory.CreateProblemDetails(controller, notFoundError)
            : successAction(entity);
    }
}
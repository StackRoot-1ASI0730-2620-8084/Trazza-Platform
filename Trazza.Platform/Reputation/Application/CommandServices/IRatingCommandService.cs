using Trazza.Platform.Reputation.Domain.Model.Aggregates;
using Trazza.Platform.Reputation.Domain.Model.Commands;
using Trazza.Platform.Shared.Application.Model;

namespace Trazza.Platform.Reputation.Application.CommandServices;

public interface IRatingCommandService
{
    Task<Result<Rating>> Handle(SubmitRatingCommand command, CancellationToken cancellationToken);
}
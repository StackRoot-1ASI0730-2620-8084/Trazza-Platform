using Trazza.Platform.Execution.Interfaces.Acl;
using Trazza.Platform.Reputation.Application.CommandServices;
using Trazza.Platform.Reputation.Domain.Model.Aggregates;
using Trazza.Platform.Reputation.Domain.Model.Commands;
using Trazza.Platform.Reputation.Domain.Model.Errors;
using Trazza.Platform.Reputation.Domain.Model.ValueObjects;
using Trazza.Platform.Reputation.Domain.Repositories;
using Trazza.Platform.Shared.Application.Model;
using Trazza.Platform.Shared.Domain.Repositories;

namespace Trazza.Platform.Reputation.Application.Internal.CommandServices;

public class RatingCommandService(
    IRatingRepository ratingRepository,
    IExecutionContextFacade executionContextFacade,
    IUnitOfWork unitOfWork)
    : IRatingCommandService
{
    public async Task<Result<Rating>> Handle(SubmitRatingCommand command, CancellationToken cancellationToken)
    {
        var score = new Score(command.Score);
        var comment = (command.Comment ?? string.Empty).Trim();

        if (!score.IsValid)
            return Result<Rating>.Failure(ReputationErrors.InvalidScore);
        if (comment.Length > Rating.MaxCommentLength)
            return Result<Rating>.Failure(ReputationErrors.CommentTooLong);

        var parties = await executionContextFacade.FetchShipmentParties(command.ShipmentId, cancellationToken);
        if (parties is null)
            return Result<Rating>.Failure(ReputationErrors.ShipmentNotFound);
        if (!parties.Value.Delivered)
            return Result<Rating>.Failure(ReputationErrors.ShipmentNotDelivered);

        var raterIsCarrier = parties.Value.CarrierId == command.RaterId.Value;
        var raterIsMerchant = parties.Value.MerchantId == command.RaterId.Value;
        if (!raterIsCarrier && !raterIsMerchant)
            return Result<Rating>.Failure(ReputationErrors.NotAParty);
        if (await ratingRepository.ExistsByShipmentIdAndRaterIdAsync(command.ShipmentId, command.RaterId,
                cancellationToken))
            return Result<Rating>.Failure(ReputationErrors.AlreadyRated);

        var target = raterIsCarrier
            ? new RatingTarget(new RatedUserId(parties.Value.MerchantId), RatingTarget.Merchant,
                parties.Value.MerchantName)
            : new RatingTarget(new RatedUserId(parties.Value.CarrierId), RatingTarget.Carrier,
                parties.Value.CarrierName);
        var raterName = raterIsCarrier ? parties.Value.CarrierName : parties.Value.MerchantName;

        var tags = (command.Tags ?? []).Select(tag => tag.Trim().ToLowerInvariant()).Distinct().ToList();
        if (tags.Any(tag => !target.AllowsTag(tag)))
            return Result<Rating>.Failure(ReputationErrors.InvalidTag);

        var rating = new Rating(command.ShipmentId, command.RaterId, raterName, target, score, tags, comment);
        await ratingRepository.AddAsync(rating, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<Rating>.Success(rating);
    }
}

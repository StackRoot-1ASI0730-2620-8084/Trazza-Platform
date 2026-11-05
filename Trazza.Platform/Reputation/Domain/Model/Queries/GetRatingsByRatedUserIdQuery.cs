using Trazza.Platform.Reputation.Domain.Model.ValueObjects;

namespace Trazza.Platform.Reputation.Domain.Model.Queries;

public record GetRatingsByRatedUserIdQuery(RatedUserId RatedUserId);
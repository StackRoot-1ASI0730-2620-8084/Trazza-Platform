# Trazza Platform - Study Guide

## Table of Contents
1. [Overview](#1-overview)
2. [Architecture Patterns](#2-architecture-patterns)
3. [Domain-Driven Design (DDD)](#3-domain-driven-design-ddd)
4. [CQRS Pattern](#4-cqrs-pattern)
5. [.NET Framework Features](#5-net-framework-features)
6. [C# Language Features](#6-c-language-features)
7. [Entity Framework Core](#7-entity-framework-core)
8. [Security & Authentication](#8-security-and-authentication)
9. [API Design](#9-api-design)
10. [Testing & Quality](#10-testing-and-quality)
11. [DevOps & Deployment](#11-devops-and-deployment)
12. [References](#12-references)

---

## 1. Overview

The **Trazza Platform** is the RESTful API of Trazza, a logistics SaaS that connects carriers who drive back empty with merchants who need to ship goods in Metropolitan Lima. It is built with **.NET 10** and **C# 14** following the same reference architecture as the course project `learning-center-platform`, applied to a richer domain with six bounded contexts and real cross-context collaboration.

### Technology Stack
- **Framework**: .NET 10 (ASP.NET Core)
- **Language**: C# 14
- **Database**: MySQL via Entity Framework Core (snake_case naming)
- **Authentication**: JSON Web Tokens (JWT) with BCrypt password hashing
- **Mediator**: Cortex Mediator for pipeline behaviors and domain event publishing
- **API Documentation**: OpenAPI with Scalar
- **Localization**: `IStringLocalizer` with English and Spanish `.resx` files
- **Containerization**: Docker multi-stage build

### Key Architectural Approaches
- **Domain-Driven Design (DDD)** with Bounded Contexts, aggregates, value objects, domain events and a domain service
- **Layered Architecture** (`Domain`, `Application`, `Infrastructure`, `Interfaces`) inside each context
- **CQRS** with command services and query services
- **Result Pattern** instead of exceptions for business rules
- **Repository and Unit of Work** over Entity Framework Core
- **Anti-Corruption Layer** facades for cross-context queries
- **Event-driven collaboration** between contexts after commit

### Bounded Contexts

| Context | Responsibility | Key Elements |
|---------|----------------|--------------|
| **Shared** | Base classes, shared kernel and cross-cutting infrastructure | `AggregateRoot`, `Result<T>`, `Error`, `Money`, `Address`, `GeoLocation` |
| **IAM** | Users, sign-up, sign-in, tokens | `User`, `EmailAddress`, `UserRole` |
| **Profiles** | Carrier and merchant profiles and the vehicle fleet | `CarrierProfile`, `Vehicle`, `MerchantProfile` |
| **Matchmaking** | Core domain: routes, requests, negotiation | `ReturnRoute`, `FreightRequest`, `MatchProposal`, `RouteMatchingService` |
| **Execution** | Shipment lifecycle and monitoring | `Shipment`, `Incident`, `ShipmentEvent` |
| **Billing** | Subscriptions, payments and receipts | `PaymentTransaction`, `Receipt`, `SubscriptionPlan` |
| **Reputation** | Ratings and reputation | `Rating`, `Score`, `RatingTarget` |

Each bounded context follows the same internal structure:

```
BoundedContext/
├── Application/
│   ├── Acl/                  # Anti-Corruption Layer implementations
│   ├── CommandServices/      # Command service interfaces
│   ├── QueryServices/        # Query service interfaces
│   └── Internal/
│       ├── CommandServices/  # Command service implementations
│       ├── QueryServices/    # Query service implementations
│       ├── EventHandlers/    # Domain event handlers
│       └── OutboundServices/ # Contracts for external services
├── Domain/
│   ├── Model/
│   │   ├── Aggregates/       # Aggregate roots (+ *Audit partial classes)
│   │   ├── Entities/         # Child entities
│   │   ├── Commands/         # Command records
│   │   ├── Queries/          # Query records
│   │   ├── Events/           # Domain events
│   │   ├── Errors/           # Error catalog of the context
│   │   └── ValueObjects/     # Value objects and strongly-typed ids
│   ├── Repositories/         # Repository contracts
│   └── Services/             # Domain services (Matchmaking)
├── Infrastructure/
│   └── Persistence/EntityFrameworkCore/
│       ├── Configuration/Extensions/  # ModelBuilder configuration
│       └── Repositories/              # Repository implementations
└── Interfaces/
    ├── Acl/                  # Facade contracts offered to other contexts
    └── Rest/
        ├── Resources/        # Request and response records
        ├── Transform/        # Assemblers and ActionResultAssembler
        └── *Controller.cs
```

---

## 2. Architecture Patterns

### 2.1 Layered Architecture

**Concept**: Each layer has one responsibility and depends only on the layers below it. The domain does not know about EF Core or HTTP.

**In the Codebase**: an aggregate only uses domain types, the repository contract lives in the domain and its implementation in infrastructure.

```csharp
public interface IReturnRouteRepository : IBaseRepository<ReturnRoute>
{
    Task<IEnumerable<ReturnRoute>> FindByCarrierIdAsNoTrackingAsync(CarrierId carrierId,
        CancellationToken cancellationToken);

    Task<IEnumerable<ReturnRoute>> ListActiveAsNoTrackingAsync(CancellationToken cancellationToken);

    Task<int> CountCreatedByCarrierSinceAsync(CarrierId carrierId, DateTimeOffset since,
        CancellationToken cancellationToken);
}
```

```csharp
public class ReturnRouteRepository(AppDbContext context)
    : BaseRepository<ReturnRoute>(context), IReturnRouteRepository
{
    public async Task<IEnumerable<ReturnRoute>> FindByCarrierIdAsNoTrackingAsync(CarrierId carrierId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<ReturnRoute>()
            .AsNoTracking()
            .Where(route => route.CarrierId == carrierId)
            .ToListAsync(cancellationToken);
    }
```

**Key Files**:
- `Trazza.Platform/Matchmaking/Domain/Repositories/IReturnRouteRepository.cs`
- `Trazza.Platform/Matchmaking/Infrastructure/Persistence/EntityFrameworkCore/Repositories/ReturnRouteRepository.cs`

**Why It Matters**: the business rules can be tested and changed without touching persistence or HTTP.

---

### 2.2 Repository Pattern

**Concept**: A collection-like abstraction over persistence. A generic base handles CRUD and specific repositories add domain queries.

**In the Codebase**: `BaseRepository<TEntity>` resolves strongly-typed keys with an expression tree so `FindByIdAsNoTrackingAsync` works for any id type.

```csharp
private Expression<Func<TEntity, bool>> HasKey(int id)
{
    var entity = Expression.Parameter(typeof(TEntity), "entity");
    var key = Expression.Property(entity, KeyName);
    var value = Expression.Constant(ToKeyValue(id), KeyType);
    return Expression.Lambda<Func<TEntity, bool>>(Expression.Equal(key, value), entity);
}
```

Specific repositories override reads to include child entities:

```csharp
public override async Task<Shipment?> FindByIdAsNoTrackingAsync(int id,
    CancellationToken cancellationToken = default)
{
    var shipmentId = new ShipmentId(id);
    return await Context.Set<Shipment>()
        .AsNoTracking()
        .AsSplitQuery()
        .Include(shipment => shipment.Events)
        .Include(shipment => shipment.Incidents)
        .FirstOrDefaultAsync(shipment => shipment.Id == shipmentId, cancellationToken);
}
```

**Key Files**:
- `Trazza.Platform/Shared/Domain/Repositories/IBaseRepository.cs`
- `Trazza.Platform/Shared/Infrastructure/Persistence/EntityFrameworkCore/Repositories/BaseRepository.cs`
- `Trazza.Platform/Profiles/Infrastructure/Persistence/EntityFrameworkCore/Repositories/CarrierProfileRepository.cs`

**Why It Matters**: application services ask for domain objects with domain language (`FindOpenByFreightRequestIdAsync`) instead of writing queries.

---

### 2.3 Unit of Work Pattern

**Concept**: All changes made in a business operation are committed together.

```csharp
using Trazza.Platform.Shared.Domain.Repositories;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}
```

In `MatchProposalCommandService`, confirming a match changes three aggregates (the proposal, the freight request and the return route) and closes competing proposals; a single `CompleteAsync` commits them atomically.

```csharp
private async Task<Error> ConfirmMatch(MatchProposal matchProposal, ENegotiationParty party,
    CancellationToken cancellationToken)
{
    var freightRequest =
        await freightRequestRepository.FindByIdAsync(matchProposal.FreightRequestId, cancellationToken);
    if (freightRequest is null) return MatchmakingErrors.FreightRequestNotFound;
    var returnRoute = await returnRouteRepository.FindByIdAsync(matchProposal.ReturnRouteId, cancellationToken);
    if (returnRoute is null) return MatchmakingErrors.ReturnRouteNotFound;

    var error = matchProposal.Confirm(party, freightRequest, returnRoute);
    if (error != Error.None) return error;

    freightRequest.MarkMatched();
    returnRoute.ReserveCapacity(freightRequest.Cargo.WeightKg, freightRequest.Cargo.VolumeM3);

    var competitors =
        await matchProposalRepository.FindOpenByFreightRequestIdAsync(freightRequest.Id, cancellationToken);
    foreach (var competitor in competitors.Where(competitor => competitor.Id != matchProposal.Id))
        competitor.Close();

    return Error.None;
}
```

**Key Files**:
- `Trazza.Platform/Shared/Domain/Repositories/IUnitOfWork.cs`
- `Trazza.Platform/Matchmaking/Application/Internal/CommandServices/MatchProposalCommandService.cs`

**Why It Matters**: the database never shows a confirmed proposal with a route that did not reserve capacity.

---

### 2.4 Dependency Injection

**Concept**: Objects receive their dependencies from the container instead of creating them.

**In the Codebase**: `Program.cs` registers every repository, service and facade per bounded context; classes receive them through primary constructors.

```csharp
builder.Services.AddScoped<IReturnRouteRepository, ReturnRouteRepository>();
builder.Services.AddScoped<IFreightRequestRepository, FreightRequestRepository>();
builder.Services.AddScoped<IMatchProposalRepository, MatchProposalRepository>();
builder.Services.AddScoped<IRouteMatchingService, RouteMatchingService>();
builder.Services.AddScoped<PublicationPolicy>();
builder.Services.AddScoped<IReturnRouteCommandService, ReturnRouteCommandService>();
builder.Services.AddScoped<IReturnRouteQueryService, ReturnRouteQueryService>();
builder.Services.AddScoped<IFreightRequestCommandService, FreightRequestCommandService>();
builder.Services.AddScoped<IFreightRequestQueryService, FreightRequestQueryService>();
builder.Services.AddScoped<IMatchProposalCommandService, MatchProposalCommandService>();
builder.Services.AddScoped<IMatchProposalQueryService, MatchProposalQueryService>();
```

```csharp
public class ReturnRouteCommandService(
    IReturnRouteRepository returnRouteRepository,
    IRouteMatchingService routeMatchingService,
    IProfilesContextFacade profilesContextFacade,
    PublicationPolicy publicationPolicy,
    IUnitOfWork unitOfWork)
    : IReturnRouteCommandService
```

**Key Files**:
- `Trazza.Platform/Program.cs`

**Why It Matters**: implementations such as `SimulatedPaymentGateway` can be replaced without touching the services that use them.

---

### 2.5 Middleware Pattern

**Concept**: Components in the HTTP pipeline that inspect or modify each request.

**In the Codebase**: `RequestAuthorizationMiddleware` validates the bearer token and stores the user in `HttpContext.Items`.

```csharp
public async Task InvokeAsync(
    HttpContext context,
    IUserQueryService userQueryService,
    ITokenService tokenService)
{
    var allowAnonymous = context.GetEndpoint()?.Metadata
        .Any(metadata => metadata.GetType() == typeof(AllowAnonymousAttribute)) ?? false;

    if (allowAnonymous)
    {
        await next(context);
        return;
    }

    var token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();

    if (!string.IsNullOrEmpty(token))
    {
        var userId = await tokenService.ValidateToken(token);
        if (userId is not null)
        {
            var user = await userQueryService.Handle(new GetUserByIdQuery(userId.Value), context.RequestAborted);
            context.Items["User"] = user;
        }
    }

    await next(context);
}
```

**Key Files**:
- `Trazza.Platform/Iam/Infrastructure/Pipeline/Middleware/Components/RequestAuthorizationMiddleware.cs`
- `Trazza.Platform/Iam/Infrastructure/Pipeline/Middleware/Extensions/RequestAuthorizationMiddlewareExtensions.cs`

**Why It Matters**: authentication is solved once for the whole API.

---

### 2.6 Assembler Pattern

**Concept**: Static mappers translate between REST resources, commands and domain objects, keeping the domain free of transport concerns.

```csharp
public static MatchProposalResource ToResourceFromEntity(MatchProposal matchProposal)
{
    return new MatchProposalResource(
        matchProposal.Id,
        matchProposal.FreightRequestId,
        matchProposal.ReturnRouteId,
        matchProposal.CarrierId,
        matchProposal.MerchantId,
        matchProposal.InitiatedBy.ToSnakeCaseValue(),
        MoneyResourceAssembler.ToResourceFromValueObject(matchProposal.CurrentRate),
        MoneyResourceAssembler.ToResourceFromNullableValueObject(matchProposal.PreviousRate),
        matchProposal.LastOfferBy.ToSnakeCaseValue(),
        matchProposal.Status.ToSnakeCaseValue(),
        MatchmakingValueObjectAssembler.ToResourceFromDetour(matchProposal.Detour),
        matchProposal.CreatedAt,
        matchProposal.UpdatedAt);
}
```

```csharp
public static Money ToValueObjectFromResource(MoneyResource? resource)
{
    return resource is null
        ? new Money(-1m)
        : new Money(Math.Round(resource.Amount, 2),
            string.IsNullOrWhiteSpace(resource.Currency) ? Money.DefaultCurrency : resource.Currency.Trim().ToUpperInvariant());
}
```

**Key Files**:
- `Trazza.Platform/Matchmaking/Interfaces/Rest/Transform`
- `Trazza.Platform/Shared/Interfaces/Rest/Transform`

**Why It Matters**: the JSON contract expected by `trazza-webapp` (snake_case statuses, nested `money` objects) can change without changing the domain.

---

### 2.7 Mediator Pattern with Pipeline Behaviors

**Concept**: A mediator dispatches requests and notifications; behaviors wrap the execution with cross-cutting logic.

```csharp
public class TransactionCommandBehavior<TCommand>(
    IUnitOfWork unitOfWork,
    ILogger<TransactionCommandBehavior<TCommand>> logger)
    : ICommandPipelineBehavior<TCommand> where TCommand : ICommand
{
    public async Task Handle(
        TCommand command,
        CommandHandlerDelegate next,
        CancellationToken ct)
    {
        await next();
        logger.LogDebug("Committing unit of work for {CommandType}", typeof(TCommand).Name);
        await unitOfWork.CompleteAsync(ct);
    }
}
```
In Trazza the mediator is also the event bus: `DispatchDomainEventsInterceptor` publishes domain events through `IMediator` and the handlers implement `IEventHandler<TEvent>`.

```csharp
using Trazza.Platform.Shared.Domain.Model.Events;
using Cortex.Mediator.Notifications;

namespace Trazza.Platform.Shared.Application.Internal.EventHandlers;

public interface IEventHandler<in TEvent> : INotificationHandler<TEvent> where TEvent : IEvent
{
}
```

**Key Files**:
- `Trazza.Platform/Shared/Infrastructure/Mediator/Cortex/Configuration`
- `Trazza.Platform/Shared/Infrastructure/Persistence/EntityFrameworkCore/Interceptors/DispatchDomainEventsInterceptor.cs`

**Why It Matters**: logging, transactions and event dispatch are written once instead of in every service.

---

### 2.8 Service Layer Pattern

**Concept**: Application services orchestrate a use case: validate input, load aggregates, call domain behavior, persist and return a result.

```csharp
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
```

**Key Files**:
- `Trazza.Platform/Reputation/Application/Internal/CommandServices/RatingCommandService.cs`

**Why It Matters**: controllers stay thin and the use case reads as a sequence of business steps.

---

### 2.9 Facade Pattern (Anti-Corruption Layer)

**Concept**: A simplified interface that one context offers to others.

```csharp
namespace Trazza.Platform.Profiles.Interfaces.Acl;

public interface IProfilesContextFacade
{
    Task<(string Label, double CapacityKg, double VolumeM3, bool Active)?> FetchCarrierVehicle(int carrierUserId,
        int vehicleId, CancellationToken cancellationToken);

    Task<(string Name, string Phone)?> FetchCarrierContact(int userId, CancellationToken cancellationToken);

    Task<(string Name, string Phone)?> FetchMerchantContact(int userId, CancellationToken cancellationToken);
}
```

```csharp
public async Task<(string Label, double CapacityKg, double VolumeM3, bool Active)?> FetchCarrierVehicle(
    int carrierUserId, int vehicleId, CancellationToken cancellationToken)
{
    var carrierProfile = await carrierProfileQueryService.Handle(
        new GetCarrierProfileByUserIdQuery(carrierUserId), cancellationToken);
    var vehicle = carrierProfile?.FindVehicle(new VehicleId(vehicleId));
    if (vehicle is null) return null;
    return (vehicle.Label, vehicle.Capacity.WeightKg, vehicle.Capacity.VolumeM3, vehicle.Active);
}
```

**Key Files**:
- `Trazza.Platform/Profiles/Interfaces/Acl/IProfilesContextFacade.cs`
- `Trazza.Platform/Billing/Interfaces/Acl/IBillingContextFacade.cs`
- `Trazza.Platform/Execution/Interfaces/Acl/IExecutionContextFacade.cs`

**Why It Matters**: Matchmaking needs the vehicle capacity but never sees the `CarrierProfile` aggregate.

---

### 2.10 Adapter Pattern (Outbound Services)

**Concept**: An interface describes what the application needs from an external system; infrastructure adapts a concrete provider.

```csharp
using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Billing.Application.Internal.OutboundServices;

public interface IPaymentGateway
{
    Task<PaymentGatewayResponse> Charge(Money amount, PaymentMethod method, CancellationToken cancellationToken);
}
```

```csharp
public class SimulatedPaymentGateway : IPaymentGateway
{
    public const string DeclinedTestCardLast4 = "0002";

    public Task<PaymentGatewayResponse> Charge(Money amount, PaymentMethod method, CancellationToken cancellationToken)
    {
        var approved = method.Last4 != DeclinedTestCardLast4 && amount.Amount > 0;
        var authorizationCode = approved
            ? $"AUT-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds():X}"
            : null;
        return Task.FromResult(new PaymentGatewayResponse(approved, authorizationCode));
    }
}
```

**Key Files**:
- `Trazza.Platform/Billing/Application/Internal/OutboundServices/IPaymentGateway.cs`
- `Trazza.Platform/Iam/Application/Internal/OutboundServices/ITokenService.cs`

**Why It Matters**: a real payment provider can be plugged in by writing one new class.

---

## 3. Domain-Driven Design (DDD)

### 3.1 Bounded Contexts

**Concept**: Each context owns a model and its language. The same idea can have different models in different contexts.

**In the Codebase**: the carrier is a `User` in IAM, a `CarrierProfile` in Profiles, a `CarrierId` in Matchmaking and Execution, and a `RatedUserId` in Reputation. Each context defines its own strongly-typed id:

```csharp
using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Execution.Domain.Model.ValueObjects;

public readonly record struct CarrierId(int Value) : IEntityId<int>
{
    public static implicit operator int(CarrierId id) => id.Value;
    public static implicit operator CarrierId(int value) => new(value);

    public override string ToString() => Value.ToString();
}
```

**Why It Matters**: changes in one context do not ripple through the others.

---

### 3.2 Aggregate Roots

**Concept**: An aggregate is a consistency boundary; only the root is referenced from outside and every change goes through its behaviors.

```csharp
using Trazza.Platform.Shared.Domain.Model.Events;

namespace Trazza.Platform.Shared.Domain.Model.Aggregates.Internal;

public abstract class AggregateRoot : IAggregateRoot
{
    private readonly List<IEvent> _domainEvents = [];

    public IReadOnlyCollection<IEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```
`CarrierProfile` protects its fleet: plates are unique inside the aggregate and vehicles are only changed through the root.
```csharp
public Error AddVehicle(LicensePlate plate, string brandModel, VehicleBodyType bodyType, LoadCapacity capacity,
    bool active = true)
{
    if (_vehicles.Any(vehicle => vehicle.Plate == plate))
        return ProfilesErrors.VehiclePlateAlreadyRegistered;

    _vehicles.Add(new Vehicle(Id, plate, brandModel, bodyType, capacity, active));
    return Error.None;
}
```
`MatchProposal` enforces the negotiation by turns:
```csharp
public Error Counter(Money rate, ENegotiationParty by)
{
    var error = EnsureCanAnswer(by);
    if (error != Error.None) return error;
    if (rate.Amount <= 0 || !rate.IsValid) return MatchmakingErrors.InvalidRate;
    if (rate == CurrentRate) return MatchmakingErrors.CounterofferSameRate;

    PreviousRate = CurrentRate;
    CurrentRate = rate;
    LastOfferBy = by;
    Status = EProposalStatus.Counteroffer;
    return Error.None;
}
```

```csharp
private Error EnsureCanAnswer(ENegotiationParty by)
{
    if (!IsNegotiating) return MatchmakingErrors.ProposalClosed;
    if (by == LastOfferBy) return MatchmakingErrors.NotYourTurn;
    return Error.None;
}
```

**Key Files**:
- `Trazza.Platform/Matchmaking/Domain/Model/Aggregates/MatchProposal.cs`
- `Trazza.Platform/Execution/Domain/Model/Aggregates/Shipment.cs`
- `Trazza.Platform/Profiles/Domain/Model/Aggregates/CarrierProfile.cs`

**Why It Matters**: no service can put a proposal in an invalid state because the aggregate refuses it.

---

### 3.3 Value Objects

**Concept**: Immutable objects defined by their attributes, with validation and behavior.

```csharp
public double DistanceTo(GeoLocation other)
{
    var deltaLatitude = ToRadians(other.Latitude - Latitude);
    var deltaLongitude = ToRadians(other.Longitude - Longitude);
    var haversine = Math.Pow(Math.Sin(deltaLatitude / 2), 2)
                    + Math.Cos(ToRadians(Latitude)) * Math.Cos(ToRadians(other.Latitude))
                    * Math.Pow(Math.Sin(deltaLongitude / 2), 2);
    return 2 * EarthRadiusKm * Math.Asin(Math.Sqrt(haversine));
}
```

```csharp
public partial record TimeWindow(string Start, string End)
{
    public TimeWindow() : this(string.Empty, string.Empty)
    {
    }

    public bool IsValid =>
        TimePattern().IsMatch(Start) && TimePattern().IsMatch(End) && ToMinutes(Start) < ToMinutes(End);

    public string Label => $"{Start} – {End}";

    public bool Overlaps(TimeWindow other) =>
```

```csharp
public static LicensePlate From(string? value)
{
    var compact = NonAlphanumeric().Replace((value ?? string.Empty).ToUpperInvariant(), string.Empty);
    return new LicensePlate(compact.Length == 6 ? $"{compact[..3]}-{compact[3..]}" : compact);
}
```

**Key Files**:
- `Trazza.Platform/Shared/Domain/Model/ValueObjects`
- `Trazza.Platform/Matchmaking/Domain/Model/ValueObjects`
- `Trazza.Platform/Billing/Domain/Model/ValueObjects/PaymentMethod.cs`

**Why It Matters**: rules like "plates have the format ABC-123" or "time windows must overlap" live in one place.

---

### 3.4 Strongly-Typed IDs

```csharp
namespace Trazza.Platform.Shared.Domain.Model.ValueObjects;

public interface IEntityId<T> where T : struct
{
    T Value { get; }
}
```

```csharp
using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

public readonly record struct ReturnRouteId(int Value) : IEntityId<int>
{
    public static implicit operator int(ReturnRouteId id) => id.Value;
    public static implicit operator ReturnRouteId(int value) => new(value);

    public override string ToString() => Value.ToString();
}
```

**Why It Matters**: the compiler rejects passing a `FreightRequestId` where a `ReturnRouteId` is expected.

---

### 3.5 Domain Events

**Concept**: Something meaningful that happened in the domain, used to trigger reactions in other parts of the system.

**In the Codebase**: confirming a match records an event with the data needed to open the shipment.

```csharp
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.Events;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.Events;

public record MatchProposalConfirmedEvent(
    MatchProposalId MatchProposalId,
    FreightRequestId FreightRequestId,
    ReturnRouteId ReturnRouteId,
    CarrierId CarrierId,
    MerchantId MerchantId,
    string VehicleLabel,
    Address Pickup,
    Address Delivery,
    DateOnly PickupDate,
    string PickupWindow,
    string CargoDescription,
    string CargoType,
    double WeightKg,
    Money Rate) : IEvent;
```

```csharp
public async Task Handle(MatchProposalConfirmedEvent domainEvent, CancellationToken cancellationToken)
{
    var command = new OpenShipmentCommand(
        new ProposalId(domainEvent.MatchProposalId.Value),
        new FreightRequestId(domainEvent.FreightRequestId.Value),
        new ReturnRouteId(domainEvent.ReturnRouteId.Value),
        new CarrierId(domainEvent.CarrierId.Value),
        new MerchantId(domainEvent.MerchantId.Value),
        domainEvent.VehicleLabel,
        domainEvent.Pickup,
        domainEvent.Delivery,
        domainEvent.PickupDate,
        domainEvent.PickupWindow,
        domainEvent.CargoDescription,
        domainEvent.CargoType,
        domainEvent.WeightKg,
        domainEvent.Rate);

    var result = await shipmentCommandService.Handle(command, cancellationToken);
    if (result.IsSuccess)
        logger.LogInformation("Shipment {Code} opened for match proposal {ProposalId}", result.Value!.Code,
            domainEvent.MatchProposalId.Value);
    else
        logger.LogWarning("Shipment for match proposal {ProposalId} was not opened: {Code}",
            domainEvent.MatchProposalId.Value, result.DomainError.Code);
}
```
The interceptor collects aggregates before saving and publishes their events **after** the commit:
```csharp
private async Task DispatchDomainEvents(CancellationToken cancellationToken = default)
{
    if (_pendingAggregateRoots.Count == 0) return;

    var domainEvents = _pendingAggregateRoots
        .SelectMany(aggregateRoot => aggregateRoot.DomainEvents)
        .ToList();

    foreach (var aggregateRoot in _pendingAggregateRoots)
    {
        aggregateRoot.ClearDomainEvents();
    }

    _pendingAggregateRoots.Clear();

    foreach (var domainEvent in domainEvents)
    {
        await mediator.PublishAsync((dynamic)domainEvent, cancellationToken);
    }
}
```

| Event | Handler | Reaction |
|:--|:--|:--|
| `UserRegisteredEvent` | `Profiles.UserRegisteredEventHandler` | Creates the profile when the document is provided |
| `UserContactUpdatedEvent` | `Profiles.UserContactUpdatedEventHandler` | Synchronizes the profile contact |
| `ReturnRouteClosedEvent` | `Matchmaking.ReturnRouteClosedEventHandler` | Closes open proposals |
| `FreightRequestCancelledEvent` | `Matchmaking.FreightRequestCancelledEventHandler` | Closes open proposals |
| `MatchProposalConfirmedEvent` | `Execution.MatchProposalConfirmedEventHandler` | Opens the shipment |
| `ShipmentDeviatedEvent` | `Execution.ShipmentDeviatedEventHandler` | Logs the deviation alert |

**Why It Matters**: Matchmaking does not depend on Execution, yet a confirmed match always produces a shipment.

---

### 3.6 Domain Services

**Concept**: Domain logic that involves several aggregates and does not belong to any of them.

```csharp
public Detour CalculateDetour(ReturnRoute returnRoute, FreightRequest freightRequest)
{
    var direct = RoadDistance(returnRoute.Origin, returnRoute.Destination);
    var viaLoad = RoadDistance(returnRoute.Origin, freightRequest.Pickup)
                  + RoadDistance(freightRequest.Pickup, freightRequest.Delivery)
                  + RoadDistance(freightRequest.Delivery, returnRoute.Destination);
    var distanceKm = Math.Max(0, viaLoad - direct);
    return Detour.Create(distanceKm, distanceKm / AverageSpeedKmh * 60);
}
```

```csharp
public MatchEvaluation Evaluate(ReturnRoute returnRoute, FreightRequest freightRequest,
    double? maxDetourKm = null)
{
    var detour = CalculateDetour(returnRoute, freightRequest);
    var allowedDetourKm = maxDetourKm ?? returnRoute.MaxDetourKm;
    var reasons = new List<string>();

    if (!returnRoute.IsActive) reasons.Add("matching.route-not-active");
    if (!freightRequest.IsOpen) reasons.Add("matching.request-not-open");
    if (returnRoute.DepartureDate != freightRequest.PickupDate) reasons.Add("matching.different-date");
    if (!returnRoute.TimeWindow.Overlaps(freightRequest.PickupWindow)) reasons.Add("matching.time-window");
    if (!returnRoute.Accepts(freightRequest.Cargo.Type)) reasons.Add("matching.cargo-type");
    if (!returnRoute.CanCarry(freightRequest.Cargo.WeightKg, freightRequest.Cargo.VolumeM3))
        reasons.Add("matching.capacity");
    if (!detour.IsWithin(allowedDetourKm)) reasons.Add("matching.detour");

    return new MatchEvaluation(reasons.Count == 0, reasons, detour);
}
```

**Key Files**:
- `Trazza.Platform/Matchmaking/Domain/Services/IRouteMatchingService.cs`
- `Trazza.Platform/Matchmaking/Domain/Services/RouteMatchingService.cs`

**Why It Matters**: the same evaluation powers proposals, load suggestions and carrier suggestions.

---

### 3.7 Entities Inside Aggregates

```csharp
public Error Resolve()
{
    if (Status == EIncidentStatus.Resolved) return ExecutionErrors.IncidentAlreadyResolved;

    Status = EIncidentStatus.Resolved;
    return Error.None;
}
```

```csharp
public Error ReportIncident(IncidentType type, string description, ReporterId reporterId, EPartyRole reporterRole,
    DateTimeOffset reportedAt)
{
    if (Status == EShipmentStatus.Cancelled) return ExecutionErrors.ShipmentCancelled;
    if (!Involves(reporterId)) return ExecutionErrors.NotAParty;
    if (IsDelivered && DeliveredAt is not null &&
        (reportedAt - DeliveredAt.Value).TotalHours > IncidentWindowHours)
        return ExecutionErrors.IncidentWindowExpired;

    _incidents.Add(new Incident(Id, type, description, reporterId, reporterRole, reportedAt));
    RecordEvent(EShipmentEventType.IncidentReported, new Dictionary<string, object> { ["type"] = type.Value });
    AddDomainEvent(new IncidentReportedEvent(Id, Code, type.Value));
    return Error.None;
}
```

**Why It Matters**: the 48-hour incident window is enforced by the shipment, the only object that knows the delivery time.

---

### 3.8 Error Catalogs

```csharp
public static readonly Error NotYourTurn =
    Error.Conflict("Matchmaking.NotYourTurn", "The last offer was yours; wait for the counterpart.");
```

```csharp
public static Error IncompatibleMatch(IEnumerable<string> reasons) =>
    Error.Validation("Matchmaking.IncompatibleMatch",
        $"The return route and the freight request are not compatible: {string.Join(", ", reasons)}.");
```

**Key Files**: `*/Domain/Model/Errors/*Errors.cs`

**Why It Matters**: every expected failure has a stable code that the frontend and the localization files can rely on.

---

## 4. CQRS Pattern

### 4.1 Command Query Responsibility Segregation

**Concept**: Writes and reads are handled by different services with different optimizations.

```csharp
using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.Commands;
using Trazza.Platform.Shared.Application.Model;

namespace Trazza.Platform.Matchmaking.Application.CommandServices;

public interface IMatchProposalCommandService
{
    Task<Result<MatchProposal>> Handle(CreateMatchProposalCommand command, CancellationToken cancellationToken);

    Task<Result<MatchProposal>> Handle(UpdateMatchProposalCommand command, CancellationToken cancellationToken);

    Task<Result<MatchProposal>> Handle(AcceptMatchProposalCommand command, CancellationToken cancellationToken);

    Task<Result<MatchProposal>> Handle(CounterMatchProposalCommand command, CancellationToken cancellationToken);

    Task<Result<MatchProposal>> Handle(RejectMatchProposalCommand command, CancellationToken cancellationToken);

    Task<Result<MatchProposal>> Handle(ConfirmMatchProposalCommand command, CancellationToken cancellationToken);

    Task<Result<int>> Handle(CloseOpenProposalsForFreightRequestCommand command, CancellationToken cancellationToken);

    Task<Result<int>> Handle(CloseOpenProposalsForReturnRouteCommand command, CancellationToken cancellationToken);
}
```

```csharp
using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.Queries;

namespace Trazza.Platform.Matchmaking.Application.QueryServices;

public interface IMatchProposalQueryService
{
    Task<IEnumerable<MatchProposal>> Handle(GetAllMatchProposalsQuery query, CancellationToken cancellationToken);

    Task<IEnumerable<MatchProposal>> Handle(GetMatchProposalsByUserIdQuery query, CancellationToken cancellationToken);

    Task<MatchProposal?> Handle(GetMatchProposalByIdQuery query, CancellationToken cancellationToken);
}
```

Commands return `Result<T>` because they can fail by business rules; queries return the entity or `null` and use non-tracking reads.

### 4.2 Commands & Queries as Records

```csharp
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.Commands;

public record CreateReturnRouteCommand(
    CarrierId CarrierId,
    VehicleId VehicleId,
    Address Origin,
    Address Destination,
    DateOnly DepartureDate,
    TimeWindow TimeWindow,
    double AvailableWeightKg,
    double AvailableVolumeM3,
    double MaxDetourKm,
    IReadOnlyList<CargoType> AcceptedCargoTypes);
```

```csharp
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.Queries;

public record GetLoadSuggestionsQuery(ReturnRouteId ReturnRouteId);
```

**Why It Matters**: records are immutable, compared by value and document each use case with a name.

---

## 5. .NET Framework Features

### 5.1 ASP.NET Core Web API

```csharp
[HttpPost("{matchProposalId:int}/confirmation")]
```

### 5.2 Minimal Hosting with Top-Level Statements

`Program.cs` builds the host, registers services and configures the pipeline without a `Startup` class.

```csharp
app.UseExceptionHandler();

var supportedCultures = new[] { "en", "es" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("AllowAllPolicy");

app.UseRequestAuthorization();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

```

### 5.3 Configuration System

Settings come from `appsettings.{Environment}.json`; `TokenSettings` is bound with the options pattern and the production connection string expands environment variables.

```json
"ConnectionStrings": {
  "DefaultConnection": "server=%DATABASE_URL%;user=%DATABASE_USER%;password=%DATABASE_PASSWORD%;database=%DATABASE_SCHEMA%"
},
```

```csharp
builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));
```

### 5.4 Localization

`ProblemDetailsFactory` localizes the detail using the error code as resource key; when no translation exists it falls back to the domain message.

```csharp
public IActionResult CreateProblemDetails(ControllerBase controller, Error error)
{
    var statusCode = ToStatusCode(error.Type);
    var localizedDetail = errorLocalizer[error.Code];

    var problemDetails = aspNetCoreProblemDetailsFactory.CreateProblemDetails(
        controller.HttpContext,
        statusCode,
        commonLocalizer[ToTitleKey(error.Type)],
        detail: localizedDetail.ResourceNotFound ? error.Message : localizedDetail.Value,
        instance: controller.HttpContext.Request.Path);

    problemDetails.Extensions["code"] = error.Code;
    return controller.StatusCode(statusCode, problemDetails);
}
```

Resources: `Trazza.Platform/Shared/Resources/Errors/ErrorMessages.resx` and `Trazza.Platform/Shared/Resources/Errors/ErrorMessages.es.resx`.

### 5.5 OpenAPI & Scalar

```csharp
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddOperationTransformer<IamBearerSecurityOperationTransformer>();
});
```

```csharp
public Task TransformAsync(
    OpenApiOperation operation,
    OpenApiOperationTransformerContext context,
    CancellationToken cancellationToken)
{
    var metadata = context.Description.ActionDescriptor.EndpointMetadata;

    if (metadata.OfType<AllowAnonymousAttribute>().Any() ||
        !metadata.OfType<AuthorizeAttribute>().Any())
    {
        return Task.CompletedTask;
    }

    operation.Security ??= [];
    operation.Security.Add(new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
    });

    return Task.CompletedTask;
}
```

### 5.6 CORS Policy

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllPolicy",
        policy => policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});
```

---

## 6. C# Language Features

### 6.1 Records

```csharp
using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Billing.Domain.Model.ValueObjects;

public record SubscriptionPlan(string Code)
{
    public const string Free = "free";
    public const string Pro = "pro";

    private static readonly IReadOnlyDictionary<string, (decimal Price, int? MonthlyPublications)> Catalog =
        new Dictionary<string, (decimal Price, int? MonthlyPublications)>
        {
            [Free] = (0m, 5),
            [Pro] = (39.90m, null)
        };

    public SubscriptionPlan() : this(string.Empty)
    {
    }

    public static IReadOnlyList<SubscriptionPlan> All => Catalog.Keys.Select(code => new SubscriptionPlan(code)).ToList();

    public static SubscriptionPlan From(string? code) => new((code ?? string.Empty).Trim().ToLowerInvariant());

    public bool IsValid => Catalog.ContainsKey(Code);

    public bool IsFree => Code == Free;

    public Money Price => new(IsValid ? Catalog[Code].Price : 0m);

    public int? MonthlyPublications => IsValid ? Catalog[Code].MonthlyPublications : 0;

    public bool AllowsPublication(int usedThisMonth) =>
        MonthlyPublications is null || usedThisMonth < MonthlyPublications;
}
```

### 6.2 Primary Constructors

```csharp
public class MatchProposalQueryService(IMatchProposalRepository matchProposalRepository) : IMatchProposalQueryService
```

### 6.3 Pattern Matching and Switch Expressions

```csharp
var error = targetStatus switch
{
    EProposalStatus.Accepted => matchProposal.Accept(answeringParty),
    EProposalStatus.Counteroffer => matchProposal.Counter(command.CurrentRate, lastOfferBy),
    EProposalStatus.Rejected => matchProposal.Reject(),
    EProposalStatus.Closed => CloseProposal(matchProposal),
    EProposalStatus.Matched => await ConfirmMatch(matchProposal, ENegotiationParty.Merchant,
        cancellationToken),
    _ => MatchmakingErrors.InvalidStatusTransition
};
```

```csharp
public bool IsActive => Status is EShipmentStatus.Matched or EShipmentStatus.PickedUp or EShipmentStatus.InTransit;
```

### 6.4 Nullable Reference Types

```csharp
public double? DeviationKm =>
    CurrentLocation is null || !IsMoving
        ? null
        : Math.Round(CurrentLocation.DistanceToSegment(Pickup.Location, Delivery.Location), 1,
            MidpointRounding.AwayFromZero);
```

### 6.5 File-Scoped Namespaces

Every file declares `namespace Trazza.Platform.<Context>.<Layer>...;` matching its folder.

### 6.6 Collection Expressions

```csharp
public static readonly IReadOnlyList<string> Values =
    ["general", "textiles", "food", "fragile", "hardware", "electronics"];
```

### 6.7 Expression-Bodied Members

```csharp
public static Detour Create(double distanceKm, double durationMinutes) =>
    new(Math.Round(Math.Max(0, distanceKm), 1, MidpointRounding.AwayFromZero),
        (int)Math.Round(Math.Max(0, durationMinutes), MidpointRounding.AwayFromZero));

public string Label => $"+{DistanceKm} km (+{DurationMinutes} min)";

public bool IsWithin(double maxDistanceKm) => DistanceKm <= maxDistanceKm;
```

### 6.8 Source-Generated Regular Expressions

```csharp
[GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]{2,}$")]
private static partial Regex EmailPattern();
```

### 6.9 Tuples

```csharp
Task<(int CarrierId, string CarrierName, int MerchantId, string MerchantName, bool Delivered)?>
    FetchShipmentParties(int shipmentId, CancellationToken cancellationToken);
```

### 6.10 Generic Constraints and Static Generic Helpers

```csharp
public static bool TryParseSnakeCase<TEnum>(string? value, out TEnum result) where TEnum : struct, Enum
{
    result = default;
    if (string.IsNullOrWhiteSpace(value) || value.Any(char.IsDigit)) return false;
    return Enum.TryParse(value.Replace("_", string.Empty), true, out result) && Enum.IsDefined(result);
}
```

### 6.11 Using Aliases

```csharp
using ProblemDetailsFactory = Trazza.Platform.Shared.Interfaces.Rest.ProblemDetails.ProblemDetailsFactory;
```

### 6.12 Dynamic Dispatch

The event interceptor publishes with the runtime type of each event so the mediator finds the handler of the concrete event type:

```csharp
await mediator.PublishAsync((dynamic)domainEvent, cancellationToken);
```

---

## 7. Entity Framework Core

### 7.1 DbContext Configuration

```csharp
public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.AddInterceptors(new AuditableEntityInterceptor());
        base.OnConfiguring(builder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyIamConfiguration();

        builder.ApplyProfilesConfiguration();

        builder.ApplyMatchmakingConfiguration();

        builder.ApplyExecutionConfiguration();

        builder.ApplyBillingConfiguration();

        builder.ApplyReputationConfiguration();

        builder.UseSnakeCaseNamingConvention();
    }
}
```

### 7.2 Fluent API: Owned Types for Value Objects

```csharp
builder.Entity<FreightRequest>().OwnsOne(f => f.Cargo, cargo =>
{
    cargo.Property(c => c.Type)
        .HasConversion(type => type.Value, value => new CargoType(value))
        .HasColumnName("CargoType")
        .IsRequired()
        .HasMaxLength(20);
    cargo.Property(c => c.WeightKg).HasColumnName("CargoWeightKg").IsRequired();
    cargo.Property(c => c.VolumeM3).HasColumnName("CargoVolumeM3").IsRequired();
    cargo.Property(c => c.Description).HasColumnName("CargoDescription").IsRequired().HasMaxLength(280);
});
```

### 7.3 Value Converters

```csharp
using Trazza.Platform.Shared.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.ValueConverters;

public class StronglyTypedIdConverter<TId, TValue>(
    Func<TId, TValue> convertToProvider,
    Func<TValue, TId> convertFromProvider)
    : ValueConverter<TId, TValue>(
        id => convertToProvider(id),
        val => convertFromProvider(val))
    where TValue : struct
{
    public StronglyTypedIdConverter(Func<TValue, TId> factory)
        : this(id => ((IEntityId<TValue>)id!).Value, factory)
    {
    }
}
```

```csharp
builder.Entity<ReturnRoute>().Property(r => r.AcceptedCargoTypes)
    .HasConversion(
        types => string.Join(",", types.Select(type => type.Value)),
        value => value.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(type => new CargoType(type))
            .ToList(),
        new ValueComparer<List<CargoType>>(
            (left, right) => left!.SequenceEqual(right!),
            types => types.Aggregate(0, (hash, type) => HashCode.Combine(hash, type.GetHashCode())),
            types => types.ToList()))
    .IsRequired()
    .HasMaxLength(120);
builder.Entity<ReturnRoute>().Property(r => r.Status).HasConversion<string>().IsRequired().HasMaxLength(20);
builder.Entity<ReturnRoute>().Property(r => r.DistanceKm).IsRequired();
```

### 7.4 Interceptors

```csharp
private static void ApplyAuditTimestamps(DbContext? context)
{
    if (context is null) return;

    var now = DateTimeOffset.UtcNow;

    foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
    {
        if (entry.State is EntityState.Added or EntityState.Modified) entry.Entity.UpdatedAt = now;
        if (entry.State == EntityState.Added) entry.Entity.CreatedAt ??= now;
    }
}
```

### 7.5 Snake Case Naming Convention

```csharp
public static void UseSnakeCaseNamingConvention(this ModelBuilder builder)
{
    foreach (var entity in builder.Model.GetEntityTypes())
    {
        var tableName = entity.GetTableName();
        if (!string.IsNullOrEmpty(tableName)) entity.SetTableName(tableName.ToPlural().ToSnakeCase());

        foreach (var property in entity.GetProperties())
            property.SetColumnName(property.GetColumnName().ToSnakeCase());
```

### 7.6 AsNoTracking and Split Queries

```csharp
public override async Task<IEnumerable<CarrierProfile>> ListAsNoTrackingAsync(
    CancellationToken cancellationToken = default)
{
    return await Context.Set<CarrierProfile>()
        .AsNoTracking()
        .AsSplitQuery()
        .Include(profile => profile.Vehicles)
        .ToListAsync(cancellationToken);
}
```

### 7.7 EF Core Migrations

```bash
dotnet ef migrations add InitialCreate --project Trazza.Platform
dotnet ef database update --project Trazza.Platform
```

Pending migrations are also applied at startup:

```csharp
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}
```

---

## 8. Security and Authentication

### 8.1 JWT Authentication

```csharp
public string GenerateToken(User user)
{
    var key = Encoding.ASCII.GetBytes(_tokenSettings.Secret);
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Sid, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email.Value),
            new Claim(ClaimTypes.Role, user.Role.Value)
        }),
        Expires = DateTime.UtcNow.AddDays(7),
        SigningCredentials =
            new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    var tokenHandler = new JsonWebTokenHandler();
    return tokenHandler.CreateToken(tokenDescriptor);
}
```

### 8.2 Password Hashing

```csharp
public class HashingService : IHashingService
{
    public string HashPassword(string password)
    {
        return BCryptNet.HashPassword(password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCryptNet.Verify(password, passwordHash);
    }
}
```

### 8.3 Authorization Filter

```csharp
public void OnAuthorization(AuthorizationFilterContext context)
{
    var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
    if (allowAnonymous) return;

    var user = context.HttpContext.Items["User"] as User;
    if (user is null) context.Result = new UnauthorizedResult();
}
```

### 8.4 Input Validation in Value Objects and Services

```csharp
if (fullName.Length < User.MinFullNameLength)
    return Result<(User user, string token)>.Failure(IamErrors.InvalidFullName);
if (!email.IsValid)
    return Result<(User user, string token)>.Failure(IamErrors.InvalidEmail);
if (!phone.IsValid)
    return Result<(User user, string token)>.Failure(IamErrors.InvalidPhone);
if (!role.IsValid)
    return Result<(User user, string token)>.Failure(IamErrors.InvalidRole);
if (password.Length < User.MinPasswordLength)
    return Result<(User user, string token)>.Failure(IamErrors.PasswordTooShort);
if (await userRepository.ExistsByEmailAsync(email, cancellationToken))
    return Result<(User user, string token)>.Failure(IamErrors.EmailAlreadyRegistered);
```

---

## 9. API Design

### 9.1 RESTful Endpoints

Resources are plural nouns and state changes that are not CRUD are modeled as sub-resources:

| Method | Route | Meaning |
|:--|:--|:--|
| `POST` | `/api/v1/match-proposals` | Send a proposal |
| `POST` | `/api/v1/match-proposals/{id}/counteroffers` | Create a counteroffer |
| `POST` | `/api/v1/match-proposals/{id}/confirmation` | Confirm the match |
| `POST` | `/api/v1/shipments/{id}/locations` | Share a location |
| `POST` | `/api/v1/shipments/{id}/incidents` | Report an incident |
| `GET` | `/api/v1/return-routes/{id}/load-suggestions` | Compatible loads |

See `docs/endpoints.md` for the full catalog of 61 endpoints.

### 9.2 Kebab-Case Routing

```csharp
public void Apply(ControllerModel controller)
{
    foreach (var selector in controller.Selectors)
        selector.AttributeRouteModel = ReplaceControllerTemplate(selector, controller.ControllerName);

    foreach (var selector in controller.Actions.SelectMany(a => a.Selectors))
        selector.AttributeRouteModel = ReplaceControllerTemplate(selector, controller.ControllerName);
}
```

### 9.3 Problem Details (RFC 9457)

```csharp
logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

var genericProblemDetails = new ProblemDetails
{
    Status = StatusCodes.Status500InternalServerError,
    Title = commonLocalizer["InternalServerError"],
    Detail = errorLocalizer["GenericError"],
    Instance = httpContext.Request.Path
};

httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
{
    HttpContext = httpContext,
    ProblemDetails = genericProblemDetails,
    Exception = exception
```

Example response:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Conflicto",
  "status": 409,
  "detail": "La última oferta fue tuya; espera a tu contraparte.",
  "instance": "/api/v1/match-proposals/7/counteroffers",
  "code": "Matchmaking.NotYourTurn"
}
```

### 9.4 Result Pattern and ActionResultAssembler

```csharp
public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<DomainErrorModel, TResult> onFailure) =>
    IsSuccess ? onSuccess(Value!) : onFailure(DomainError);
```

```csharp
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
```

### 9.5 Frontend-Compatible PUT as Domain Transitions

`PUT /match-proposals/{id}` receives the full resource from the web application and translates the requested status into aggregate behavior, so the backend keeps enforcing the rules (see ADR-010).

### 9.6 API Versioning

All routes are prefixed with `api/v1`, leaving room for a future `v2`.

---

## 10. Testing and Quality

### 10.1 Mediator Pipeline Behaviors

```csharp
public async Task Handle(
    TCommand command,
    CommandHandlerDelegate next,
    CancellationToken ct)
{
    try
    {
        await next();
    }
    catch (OperationCanceledException)
    {
        logger.LogWarning("Command {CommandName} was cancelled.", typeof(TCommand).Name);
        throw;
    }
    catch (DbUpdateException ex)
    {
        logger.LogError(ex, "Database update error during command {CommandName}.", typeof(TCommand).Name);
        throw;
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unhandled error during command {CommandName}.", typeof(TCommand).Name);
        throw;
    }
}
```

### 10.2 Code Organization

- One type per file, folders mirror namespaces.
- Audit fields in `*Audit.cs` partial classes keep aggregates focused on behavior.
- No comments in source code: names express intent (`EnsureCanAnswer`, `ReserveCapacity`, `IsSamePlaceAs`).

### 10.3 Testability

Domain behaviors return `Error` values and depend on no infrastructure, so they can be unit tested directly, for example `MatchProposal.Counter` with the same rate must return `MatchmakingErrors.CounterofferSameRate`.

---

## 11. DevOps and Deployment

### 11.1 Docker Multi-Stage Build

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder
WORKDIR /app
COPY Trazza.Platform/*.csproj Trazza.Platform/
RUN dotnet restore ./Trazza.Platform
COPY . .
RUN dotnet publish ./Trazza.Platform -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=builder /app/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "Trazza.Platform.dll"]
```

### 11.2 Environment Variable Configuration

`appsettings.Production.json` uses `%DATABASE_URL%`, `%DATABASE_USER%`, `%DATABASE_PASSWORD%` and `%DATABASE_SCHEMA%`, expanded with `Environment.ExpandEnvironmentVariables` in `Program.cs`.

### 11.3 Database Migration Strategy

Migrations are generated with `dotnet ef` and applied automatically on startup with `context.Database.Migrate()`.

---

## 12. References

### Official Documentation
- [ASP.NET Core documentation](https://learn.microsoft.com/en-us/aspnet/core/) — Web API, middleware, configuration and hosting.
- [Entity Framework Core documentation](https://learn.microsoft.com/en-us/ef/core/) — modeling, owned types, value converters and interceptors.
- [C# documentation](https://learn.microsoft.com/en-us/dotnet/csharp/) — records, pattern matching and primary constructors.
- [Handle errors in ASP.NET Core APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling-api) — `IExceptionHandler` and problem details.
- [RFC 9457: Problem Details for HTTP APIs](https://www.rfc-editor.org/rfc/rfc9457) — standard error format.
- [Scalar for ASP.NET Core](https://github.com/scalar/scalar/tree/main/integrations/aspnetcore) — interactive API reference.
- [Cortex Mediator](https://github.com/buildersoftio/cortex) — mediator, pipeline behaviors and notifications.
- [MySQL Connector/NET for EF Core](https://dev.mysql.com/doc/connector-net/en/connector-net-entityframework-core.html) — MySQL provider.

### Books
- **Domain-Driven Design: Tackling Complexity in the Heart of Software** — Eric Evans (2003). The original description of bounded contexts, aggregates and ubiquitous language.
- **Implementing Domain-Driven Design** — Vaughn Vernon (2013). Practical guidance on aggregates, domain events and context mapping.
- **Domain-Driven Design Distilled** — Vaughn Vernon (2016). A short introduction to strategic and tactical DDD.
- **Patterns of Enterprise Application Architecture** — Martin Fowler (2002). Repository, Unit of Work, Data Mapper and Service Layer.
- **Clean Architecture** — Robert C. Martin (2017). Dependency rule and layer separation.
- **Learning Domain-Driven Design** — Vlad Khononov (2021). Modern take on bounded contexts and integration patterns.

### Online Resources
- [Microsoft: Design a DDD-oriented microservice](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/ddd-oriented-microservice) — DDD and CQRS in .NET.
- [Martin Fowler: CQRS](https://martinfowler.com/bliki/CQRS.html) — overview of the pattern.
- [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) — commit message convention used in this repository.
- [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) — changelog format.

### Key Concepts to Research
1. Aggregate design and consistency boundaries.
2. Domain events versus integration events and the outbox pattern.
3. Eventual consistency between bounded contexts.
4. Value objects and owned types in EF Core.
5. Result pattern versus exceptions.
6. JWT structure, signing and expiration.
7. RFC 9457 problem details and API error design.
8. Geospatial distance calculations (haversine) and their limits.

### Exercises
1. Add `GET /api/v1/users/{userId}/publications` returning the routes and requests published this month.
2. Write unit tests for `MatchProposal` covering accept, counteroffer, out-of-turn and confirmation rules.
3. Add a `VehicleDeactivatedEvent` that closes the active return routes of that vehicle.
4. Persist `ShipmentDeviatedEvent` notifications and expose them with `GET /api/v1/users/{userId}/notifications`.
5. Replace `SimulatedPaymentGateway` with an adapter for a real payment provider sandbox.
6. Add pagination and filtering by district to `GET /api/v1/freight-requests`.
7. Route commands through Cortex Mediator `ICommandHandler` so the pipeline behaviors wrap every command.
8. Implement the outbox pattern so domain events survive a crash between commit and dispatch.


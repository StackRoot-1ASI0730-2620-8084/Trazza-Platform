# Architecture Decision Records (ADRs)

This document records the key architectural decisions made for the **Trazza Platform** RESTful Web Services, detailing the context, options considered, decisions and consequences. The backend serves the Trazza web application (`trazza-webapp`) and follows the conventions of the reference project `learning-center-platform`.

---

## ADR Index

1. [ADR-001: Domain-Driven Design with the Trazza Bounded Contexts](#adr-001-domain-driven-design-with-the-trazza-bounded-contexts)
2. [ADR-002: CQRS with Command and Query Services and Cortex Mediator](#adr-002-cqrs-with-command-and-query-services-and-cortex-mediator)
3. [ADR-003: Result Pattern and Typed Error Model Instead of Business Exceptions](#adr-003-result-pattern-and-typed-error-model-instead-of-business-exceptions)
4. [ADR-004: Strongly-Typed Identifiers with Readonly Record Structs](#adr-004-strongly-typed-identifiers-with-readonly-record-structs)
5. [ADR-005: Non-Tracking Reads in Query Services](#adr-005-non-tracking-reads-in-query-services)
6. [ADR-006: Domain Events Accumulated in Aggregates and Dispatched After Commit](#adr-006-domain-events-accumulated-in-aggregates-and-dispatched-after-commit)
7. [ADR-007: Shared Kernel with Money, Address and GeoLocation](#adr-007-shared-kernel-with-money-address-and-geolocation)
8. [ADR-008: Route Matching as a Domain Service](#adr-008-route-matching-as-a-domain-service)
9. [ADR-009: Anti-Corruption Layer Facades Between Bounded Contexts](#adr-009-anti-corruption-layer-facades-between-bounded-contexts)
10. [ADR-010: Frontend-Compatible PUT Endpoints Translated into Domain Transitions](#adr-010-frontend-compatible-put-endpoints-translated-into-domain-transitions)
11. [ADR-011: IAM with BCrypt, JWT and a Request Authorization Middleware](#adr-011-iam-with-bcrypt-jwt-and-a-request-authorization-middleware)
12. [ADR-012: IExceptionHandler and RFC 9457 Problem Details with Localized Messages](#adr-012-iexceptionhandler-and-rfc-9457-problem-details-with-localized-messages)
13. [ADR-013: EF Core with MySQL, Snake Case Naming and Audit Interceptor](#adr-013-ef-core-with-mysql-snake-case-naming-and-audit-interceptor)
14. [ADR-014: OpenAPI Documentation with Scalar](#adr-014-openapi-documentation-with-scalar)
15. [ADR-015: External Payment Gateway Behind an Outbound Service](#adr-015-external-payment-gateway-behind-an-outbound-service)

---

## ADR-001: Domain-Driven Design with the Trazza Bounded Contexts

- **Status:** Accepted
- **Date:** 2026-11-03

### Context
Trazza connects carriers that travel with empty return trips and merchants that need to ship goods inside Metropolitan Lima. The domain covers identity, profiles and fleets, route matching and rate negotiation, trip execution and monitoring, subscription billing and reputation. Each area has its own language and rules.

### Decision
Organize `Trazza.Platform` into the same bounded contexts used by the web application, each split into `Domain`, `Application`, `Infrastructure` and `Interfaces`:

| Bounded Context | Aggregates | Main Value Objects |
|:--|:--|:--|
| `Iam` | `User` | `EmailAddress`, `PhoneNumber`, `UserRole` |
| `Profiles` | `CarrierProfile` (entity `Vehicle`), `MerchantProfile` | `Dni`, `Ruc`, `LicensePlate`, `LoadCapacity`, `VehicleBodyType` |
| `Matchmaking` | `ReturnRoute`, `FreightRequest`, `MatchProposal` | `TimeWindow`, `Cargo`, `CargoType`, `Detour`, `ERouteStatus`, `ERequestStatus`, `EProposalStatus` |
| `Execution` | `Shipment` (entities `Incident`, `ShipmentEvent`) | `EShipmentStatus`, `IncidentType`, `EPartyRole` |
| `Billing` | `PaymentTransaction`, `Receipt` | `SubscriptionPlan`, `PaymentMethod`, `ReceiptType`, `EPaymentStatus` |
| `Reputation` | `Rating` | `Score`, `RatingTarget`, `ReputationSummary` |
| `Shared` | `AggregateRoot` | `Money`, `Address`, `GeoLocation`, `LimaDistricts`, `LimaCalendar` |

### Consequences
- **Positive:** the backend speaks the same ubiquitous language as the frontend; each context evolves independently.
- **Negative:** more types and mapping code between resources, commands and aggregates.

---

## ADR-002: CQRS with Command and Query Services and Cortex Mediator

- **Status:** Accepted
- **Date:** 2026-11-03

### Context
Write operations enforce invariants and change state, while reads must be fast and side-effect free.

### Decision
- Every context exposes `I*CommandService` (state changes, returns `Result<T>`) and `I*QueryService` (reads, returns entities or `null`).
- Commands and queries are immutable records in `Domain/Model/Commands` and `Domain/Model/Queries`.
- `Cortex.Mediator` is registered with the pipeline behaviors `LoggingCommandBehavior`, `ExceptionHandlingCommandBehavior` and `TransactionCommandBehavior`, and it publishes domain events to `IEventHandler<TEvent>` implementations.

### Consequences
- **Positive:** clear separation of responsibilities and cross-cutting behaviors in one place.
- **Negative:** one command or query record per use case.

---

## ADR-003: Result Pattern and Typed Error Model Instead of Business Exceptions

- **Status:** Accepted
- **Date:** 2026-11-03

### Context
Throwing exceptions for expected business outcomes (a duplicated email, an out-of-turn counteroffer) mixes control flow with failures and hides the HTTP status to return.

### Decision
- `Error(Code, Message, EErrorType)` with `Validation`, `NotFound`, `Conflict`, `Unauthorized`, `Forbidden`, `Failure` and `Unexpected`.
- Each context declares its catalog (`IamErrors`, `ProfilesErrors`, `MatchmakingErrors`, `ExecutionErrors`, `BillingErrors`, `ReputationErrors`).
- Aggregate behaviors return `Error.None` or a domain error (`MatchProposal.Counter`, `Shipment.ConfirmDelivery`).
- Command services return `Result<T>`; controllers resolve it with `.Match()` inside the context `*ActionResultAssembler`, which delegates failures to `ProblemDetailsFactory.CreateProblemDetails(controller, error)`.

### Consequences
- **Positive:** predictable responses, status codes derived from `EErrorType`, no try/catch in controllers.
- **Negative:** callers must check the result of each domain operation.

---

## ADR-004: Strongly-Typed Identifiers with Readonly Record Structs

- **Status:** Accepted
- **Date:** 2026-11-03

### Context
Plain `int` identifiers allow mixing a `ReturnRouteId` with a `FreightRequestId` without any compiler error.

### Decision
Every identifier is a `readonly record struct` implementing `IEntityId<int>` with implicit conversions (`UserId`, `CarrierProfileId`, `VehicleId`, `ReturnRouteId`, `FreightRequestId`, `MatchProposalId`, `ShipmentId`, `IncidentId`, `PaymentTransactionId`, `ReceiptId`, `RatingId`). External references are typed inside each context (`CarrierId`, `MerchantId`, `RaterId`, `RatedUserId`). EF Core persists them with `StronglyTypedIdConverter<TId, int>`.

### Consequences
- **Positive:** no primitive obsession and zero allocation.
- **Negative:** each identifier needs a converter in the model configuration.

---

## ADR-005: Non-Tracking Reads in Query Services

- **Status:** Accepted
- **Date:** 2026-11-03

### Decision
Query services only call repository methods that use `.AsNoTracking()` (and `.AsSplitQuery()` when including collections such as vehicles, shipment events and incidents). Command services load tracked aggregates.

### Consequences
- **Positive:** lower memory use and faster reads.
- **Negative:** entities returned by queries cannot be modified and saved directly.

---

## ADR-006: Domain Events Accumulated in Aggregates and Dispatched After Commit

- **Status:** Accepted
- **Date:** 2026-11-03

### Context
Some use cases cross contexts: a registration may create a profile, updating a user contact must update the profile, confirming a match must open a shipment, closing a route must close its open proposals.

### Decision
- Aggregates inherit `AggregateRoot` and record events with `AddDomainEvent`.
- `DispatchDomainEventsInterceptor` collects aggregates with events in `SavingChanges` and publishes the events through Cortex Mediator in `SavedChanges`, that is, **after the commit**, so the events see the generated identifiers and only successful transactions are propagated. Pending events are discarded if `SaveChanges` fails.
- Events and handlers:

| Event | Raised by | Handler | Effect |
|:--|:--|:--|:--|
| `UserRegisteredEvent` | `User` | `Profiles.UserRegisteredEventHandler` | Creates the carrier or merchant profile when the sign-up includes the document |
| `UserContactUpdatedEvent` | `User.UpdateContact` | `Profiles.UserContactUpdatedEventHandler` | Synchronizes name and phone in the profile |
| `ReturnRouteClosedEvent` | `ReturnRoute.Close` | `Matchmaking.ReturnRouteClosedEventHandler` | Closes open proposals of the route |
| `FreightRequestCancelledEvent` | `FreightRequest.Cancel` | `Matchmaking.FreightRequestCancelledEventHandler` | Closes open proposals of the request |
| `MatchProposalConfirmedEvent` | `MatchProposal.Confirm` | `Execution.MatchProposalConfirmedEventHandler` | Opens the `Shipment` |
| `ShipmentDeviatedEvent` | `Shipment.UpdateLocation` | `Execution.ShipmentDeviatedEventHandler` | Logs the route deviation alert |

### Consequences
- **Positive:** contexts stay decoupled and reactions happen only after a successful commit.
- **Negative:** reactions run in a second save; they are eventually consistent with the originating transaction.

---

## ADR-007: Shared Kernel with Money, Address and GeoLocation

- **Status:** Accepted
- **Date:** 2026-11-03

### Decision
The `Shared` context contains the value objects used by several contexts, mirroring the frontend shared kernel:
- `Money(Amount, Currency)` in PEN, with `Add`, `Multiply`, `IsGreaterThan` and `Formatted` (`S/ 39.90`).
- `Address(Street, District)` validated against `LimaDistricts`, exposing its approximate `Location`.
- `GeoLocation(Latitude, Longitude)` with haversine `DistanceTo`, `MoveTowards` and `DistanceToSegment`.
- `LimaCalendar` with the Lima time offset (UTC-5) to compute today and the start of the month.

Value objects expose `IsValid` instead of throwing; command services translate invalid values into `Result` failures.

---

## ADR-008: Route Matching as a Domain Service

- **Status:** Accepted
- **Date:** 2026-11-03

### Context
Compatibility between a return route and a freight request depends on both aggregates and does not belong to either of them.

### Decision
`IRouteMatchingService` / `RouteMatchingService` in `Matchmaking/Domain/Services` estimates trips (`road factor 1.3`, `30 km/h`), calculates the detour (`origin → pickup → delivery → destination` minus the direct trip) and evaluates compatibility: route active, request open, same date, overlapping time windows, accepted cargo type, enough capacity and detour within the maximum. It is used to create proposals and to serve `load-suggestions` and `carrier-suggestions`.

---

## ADR-009: Anti-Corruption Layer Facades Between Bounded Contexts

- **Status:** Accepted
- **Date:** 2026-11-03

### Decision
Contexts query each other only through facades declared in `Interfaces/Acl` and implemented in `Application/Acl`, which return primitives or tuples:

| Facade | Consumers | Purpose |
|:--|:--|:--|
| `IIamContextFacade` | Profiles, Billing | User id by email, role and existence |
| `IProfilesContextFacade` | Matchmaking, Execution | Vehicle capacity and label, carrier and merchant contacts |
| `IBillingContextFacade` | Matchmaking | Monthly publication limit of the current plan |
| `IExecutionContextFacade` | Reputation | Parties and delivery state of a shipment |

---

## ADR-010: Frontend-Compatible PUT Endpoints Translated into Domain Transitions

- **Status:** Accepted
- **Date:** 2026-11-03

### Context
The web application persists aggregates by sending the full resource with `PUT` after applying the behavior locally (for example, a proposal with `status: "accepted"`).

### Decision
- `PUT` endpoints never overwrite state blindly. They translate the requested status into the corresponding aggregate behavior (`Accept`, `Counter`, `Reject`, `Confirm`, `Close`, `ConfirmPickup`, `UpdateLocation`, `ConfirmDelivery`, `ConfirmReception`, `Cancel`) so the backend enforces the same invariants. Requests that do not change state are idempotent.
- Explicit action endpoints are also exposed (`/acceptance`, `/counteroffers`, `/confirmation`, `/pickup`, `/delivery`, `/incidents`).
- `POST /shipments` is idempotent per proposal because the shipment is already opened by `MatchProposalConfirmedEvent`.

---

## ADR-011: IAM with BCrypt, JWT and a Request Authorization Middleware

- **Status:** Accepted
- **Date:** 2026-11-03

### Decision
- Passwords are hashed with BCrypt (`IHashingService`).
- `POST /authentication/sign-in` and `POST /authentication/sign-up` return the user with a JWT signed with HMAC SHA-256 (`ITokenService`, 7 days, claims `sid`, `email`, `role`).
- `RequestAuthorizationMiddleware` validates the bearer token and places the user in `HttpContext.Items["User"]`; the custom `[Authorize]` filter rejects requests without user, and `[AllowAnonymous]` opens public endpoints.

---

## ADR-012: IExceptionHandler and RFC 9457 Problem Details with Localized Messages

- **Status:** Accepted
- **Date:** 2026-11-03

### Decision
- `GlobalExceptionHandler` implements `IExceptionHandler` and writes `ProblemDetails` for unexpected exceptions.
- `ProblemDetailsFactory` maps `EErrorType` to the HTTP status, localizes the title with `CommonMessages` and the detail with `ErrorMessages` using the error code as key, and adds the `code` extension.
- Messages live in `.resx` files in English and Spanish; the culture is selected by `Accept-Language`.

---

## ADR-013: EF Core with MySQL, Snake Case Naming and Audit Interceptor

- **Status:** Accepted
- **Date:** 2026-11-03

### Decision
- `AppDbContext` applies each context configuration (`ApplyIamConfiguration`, `ApplyProfilesConfiguration`, `ApplyMatchmakingConfiguration`, `ApplyExecutionConfiguration`, `ApplyBillingConfiguration`, `ApplyReputationConfiguration`) and `UseSnakeCaseNamingConvention()` with pluralized table names.
- Value objects are owned types (`Address`, `Money`, `TimeWindow`, `Cargo`, `Detour`, `GeoLocation`, `PaymentMethod`, `RatingTarget`) or value conversions (`EmailAddress`, `Dni`, `LicensePlate`, `CargoType` lists).
- `AuditableEntityInterceptor` fills `CreatedAt` and `UpdatedAt` for every `IAuditableEntity`.
- Migrations are applied on startup with `Database.Migrate()`.

---

## ADR-014: OpenAPI Documentation with Scalar

- **Status:** Accepted
- **Date:** 2026-11-03

### Decision
`Microsoft.AspNetCore.OpenApi` generates the document with `BearerSecuritySchemeTransformer` and `IamBearerSecurityOperationTransformer` (security requirement only on protected operations). `Scalar.AspNetCore` serves the interactive reference at `/scalar/v1`. Every action declares `EndpointSummary`, `EndpointDescription` and `ProducesResponseType`.

---

## ADR-015: External Payment Gateway Behind an Outbound Service

- **Status:** Accepted
- **Date:** 2026-11-03

### Decision
`IPaymentGateway` (`Billing/Application/Internal/OutboundServices`) abstracts the card processor. `SimulatedPaymentGateway` approves every card except the test card ending in `0002`, matching the frontend gateway. Replacing it with a real provider only requires a new infrastructure adapter.

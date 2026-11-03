# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Added
- Nothing yet.

---

## [1.0.0] - 2026-11-03

### Added
- **Shared Kernel:**
  - Added `AggregateRoot`, `IAggregateRoot`, `IEvent`, `IAuditableEntity` and `IEntityId<T>` domain abstractions.
  - Added `Result<T>` with `.Match()` and the `Error` record with `EErrorType` classification.
  - Added `Money`, `Address`, `GeoLocation`, `LimaDistricts` and `LimaCalendar` value objects aligned with the web application shared kernel.
  - Added `StronglyTypedIdConverter<TId, TValue>`, `AuditableEntityInterceptor` and `DispatchDomainEventsInterceptor` (dispatch after commit).
  - Added `LoggingCommandBehavior`, `ExceptionHandlingCommandBehavior` and `TransactionCommandBehavior` for Cortex Mediator.
  - Added `GlobalExceptionHandler` and `ProblemDetailsFactory` emitting RFC 9457 problem details with the domain error code.
  - Added English and Spanish error messages in `ErrorMessages.resx` and `ErrorMessages.es.resx`.
- **IAM Bounded Context:**
  - Added `User` aggregate with `EmailAddress`, `PhoneNumber` and `UserRole` value objects.
  - Added `POST /api/v1/authentication/sign-up` and `POST /api/v1/authentication/sign-in` returning the user with a JWT bearer token.
  - Added `GET /api/v1/users`, `GET /api/v1/users/{userId}` and `PUT /api/v1/users/{userId}`.
  - Added BCrypt hashing, JWT token service and `RequestAuthorizationMiddleware` with `[Authorize]` and `[AllowAnonymous]`.
  - Added `UserRegisteredEvent` and `UserContactUpdatedEvent`.
- **Profiles Bounded Context:**
  - Added `CarrierProfile` aggregate with `Vehicle` entity and `MerchantProfile` aggregate.
  - Added `Dni`, `Ruc`, `LicensePlate`, `LoadCapacity` and `VehicleBodyType` value objects.
  - Added carrier and merchant profile endpoints and vehicle management endpoints.
  - Added event handlers that create profiles after registration and synchronize contact data.
- **Matchmaking & Routing Bounded Context:**
  - Added `ReturnRoute`, `FreightRequest` and `MatchProposal` aggregates with `TimeWindow`, `Cargo`, `CargoType` and `Detour` value objects.
  - Added `RouteMatchingService` domain service for trip estimation, detour calculation and compatibility evaluation.
  - Added negotiation by turns (accept, counteroffer, reject) and match confirmation that reserves capacity and closes competing proposals.
  - Added load suggestions and carrier suggestions endpoints.
  - Added the Free plan monthly publication limit through `IBillingContextFacade`.
- **Service Execution & Monitoring Bounded Context:**
  - Added `Shipment` aggregate with `Incident` and `ShipmentEvent` entities.
  - Added pickup, live location with deviation detection, delivery, reception, cancellation and incident endpoints.
  - Added `MatchProposalConfirmedEventHandler` to open the shipment after a confirmed match.
- **Payment & Billing Bounded Context:**
  - Added `PaymentTransaction` and `Receipt` aggregates with `SubscriptionPlan`, `PaymentMethod` and `ReceiptType` value objects.
  - Added `IPaymentGateway` with `SimulatedPaymentGateway`.
  - Added payment transaction, receipt, subscription plan and user subscription endpoints.
- **Loyalty & Reputation Bounded Context:**
  - Added `Rating` aggregate with `Score` and `RatingTarget` value objects.
  - Added rating endpoints and the user reputation summary endpoint.
- **OpenAPI & Scalar Reference:**
  - Added `BearerSecuritySchemeTransformer` and `IamBearerSecurityOperationTransformer` for interactive JWT authorization at `/scalar/v1`.
- **Documentation:**
  - Added `README.md`, `CONTRIBUTING.md`, `LICENSE.md` and `CHANGELOG.md`.
  - Added `docs/adrs.md`, `docs/user-stories.md`, `docs/endpoints.md`, `docs/class-diagram.puml`, `docs/software-architecture.dsl`, `docs/study-guide.md` and `docs/study-guide-spec.md`.

### Changed
- Nothing yet.

### Fixed
- Nothing yet.

### Deprecated
- Nothing yet.

### Removed
- Nothing yet.

### Security
- Passwords are stored as BCrypt hashes and protected endpoints require a JWT bearer token.

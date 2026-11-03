# Trazza Platform

[![Release](https://img.shields.io/badge/release-v1.0.0-blue.svg)](CHANGELOG.md)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-14.0-blue.svg)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE.md)

## Project Overview

The **Trazza Platform** is the RESTful backend of **Trazza**, a logistics SaaS that connects **carriers** who drive back empty after a delivery with **merchants** (small businesses and entrepreneurs) who need to ship goods inside Metropolitan Lima. Carriers publish *return routes* with their free capacity, merchants publish *freight requests*, the platform suggests compatible loads, lets both parties negotiate the rate, tracks the shipment until delivery, bills subscriptions and builds the reputation of both sides.

Built with **.NET 10** and **C# 14**, it adheres to **Domain-Driven Design (DDD)** tactical patterns and implements **Command Query Responsibility Segregation (CQRS)** with pipeline behaviors. It serves the Vue 3 web application `trazza-webapp`.

The platform is structured into decoupled **Bounded Contexts** (`IAM`, `Profiles`, `Matchmaking`, `Execution`, `Billing`, `Reputation`, `Shared`), ensuring clear separation of concerns, high maintainability, and robust transactional consistency.

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Domain-Driven Design (DDD) Concepts](#domain-driven-design-ddd-concepts)
3. [Key Features & Best Practices](#key-features--best-practices)
4. [Bounded Contexts](#bounded-contexts)
   * [IAM (Identity & Access Management)](#iam-identity-and-access-management)
   * [Profiles](#profiles)
   * [Matchmaking & Routing](#matchmaking--routing)
   * [Service Execution & Monitoring](#service-execution--monitoring)
   * [Payment & Billing](#payment--billing)
   * [Loyalty & Reputation](#loyalty--reputation)
   * [Shared Kernel](#shared-kernel)
5. [Technologies Used](#technologies-used)
6. [Getting Started](#getting-started)
   * [Prerequisites](#prerequisites)
   * [Setup Instructions](#setup-instructions)
7. [Project Structure](#project-structure)
8. [Documentation](#documentation)
9. [Team](#team)
10. [Contributing & Changelog](#contributing--changelog)
11. [License](#license)

---

## Architecture Overview

The platform architecture follows **Domain-Driven Design (DDD)** and **Command Query Responsibility Segregation (CQRS)**:

* **Domain Layer**: Encapsulates aggregates (`AggregateRoot`), entities, value objects, domain events (`IEvent`), domain services (`RouteMatchingService`), error catalogs and repository contracts. Contains zero outward dependencies.
* **Application Layer**: Coordinates domain operations through explicit **Command Services** (write side) and **Query Services** (read side), event handlers that react to domain events, outbound service contracts (`ITokenService`, `IHashingService`, `IPaymentGateway`) and ACL facades.
* **Infrastructure Layer**: Implements persistence via Entity Framework Core (MySQL), strongly-typed ID value converters, JWT token services, BCrypt hashing, the simulated payment gateway and the interceptor-based domain event dispatcher.
* **Interfaces Layer (Presentation)**: Exposes RESTful controllers, resources and assemblers, RFC 9457 `ProblemDetails` error responses and ACL facade contracts.

---

## Domain-Driven Design (DDD) Concepts

* **Bounded Contexts**: Independent business domains (`IAM`, `Profiles`, `Matchmaking`, `Execution`, `Billing`, `Reputation`, and `Shared`).
* **Aggregates & Invariant Protection**: Aggregates (`User`, `CarrierProfile`, `MerchantProfile`, `ReturnRoute`, `FreightRequest`, `MatchProposal`, `Shipment`, `PaymentTransaction`, `Receipt`, `Rating`) inherit from `AggregateRoot` and guard internal state consistency, returning domain `Error` values when an invariant would be broken.
* **Strongly-Typed IDs**: Readonly record structs (`UserId`, `CarrierProfileId`, `VehicleId`, `ReturnRouteId`, `FreightRequestId`, `MatchProposalId`, `ShipmentId`, `IncidentId`, `PaymentTransactionId`, `ReceiptId`, `RatingId`) implementing `IEntityId<int>`.
* **Domain Events**: Aggregates record events (`UserRegisteredEvent`, `UserContactUpdatedEvent`, `ReturnRouteClosedEvent`, `FreightRequestCancelledEvent`, `MatchProposalConfirmedEvent`, `ShipmentDeviatedEvent`, `PaymentTransactionPaidEvent`, `RatingSubmittedEvent`) dispatched through `DispatchDomainEventsInterceptor` and `Cortex.Mediator` after a successful commit.
* **Value Objects**: Immutable records (`Money`, `Address`, `GeoLocation`, `EmailAddress`, `Dni`, `Ruc`, `LicensePlate`, `LoadCapacity`, `TimeWindow`, `Cargo`, `CargoType`, `Detour`, `IncidentType`, `SubscriptionPlan`, `PaymentMethod`, `ReceiptType`, `Score`, `RatingTarget`).
* **Domain Service**: `RouteMatchingService` evaluates capacity, time windows, cargo types and detours between return routes and freight requests.
* **Anti-Corruption Layer (ACL)**: Facades (`IIamContextFacade`, `IProfilesContextFacade`, `IBillingContextFacade`, `IExecutionContextFacade`) isolate cross-context communication.

---

## Key Features & Best Practices

* **CQRS with Mediator Pipeline Behaviors**:
  * `TransactionCommandBehavior`: Automatically commits changes via `IUnitOfWork.CompleteAsync()` on successful command execution.
  * `ExceptionHandlingCommandBehavior`: Centralizes database and concurrency exception mapping.
  * `LoggingCommandBehavior`: Structured execution tracing.
* **Query Performance Optimizations**:
  * Read services use `.AsNoTracking()` and `.AsSplitQuery()` to eliminate change-tracking overhead and prevent Cartesian joins.
* **Functional Result Pattern**:
  * Operations return `Result<T>` and strongly-typed `Error` records with `EErrorType` classification (`Validation`, `NotFound`, `Conflict`, `Unauthorized`, `Forbidden`, `Failure`, `Unexpected`), leveraging `.Match()` in the `*ActionResultAssembler` of each context.
* **Interactive Scalar OpenAPI Documentation**:
  * Built on `Microsoft.AspNetCore.OpenApi` and `Scalar.AspNetCore` at `/scalar/v1`, with JWT Bearer authorization through `BearerSecuritySchemeTransformer` and `IamBearerSecurityOperationTransformer`.
* **Standardized Error Handling**:
  * ASP.NET Core `IExceptionHandler` (`GlobalExceptionHandler`) emitting RFC 9457 `ProblemDetails`, enriched with the domain error `code`.
* **Internationalization (i18n)**:
  * Localized error messages in English and Spanish via `IStringLocalizer` and `.resx` resource files, selected with `Accept-Language`.
* **Frontend Compatibility**:
  * Routes, resources and enumerated values (`snake_case`) match the assemblers of `trazza-webapp`; `PUT` requests are translated into domain transitions.

---

## Bounded Contexts

### IAM (Identity and Access Management)
* **Scope**: Carrier and merchant registration, authentication, JWT issuance and password security.
* **Aggregates**: `User`
* **Value Objects**: `UserId`, `EmailAddress`, `PhoneNumber`, `UserRole`
* **Domain Events**: `UserRegisteredEvent`, `UserContactUpdatedEvent`
* **RESTful Endpoints**:
  * `POST /api/v1/authentication/sign-up` — Register user (`201 Created` with `AuthenticatedUserResource`)
  * `POST /api/v1/authentication/sign-in` — Authenticate (`200 OK` with `AuthenticatedUserResource`)
  * `GET /api/v1/users/{userId}` — Get user by id
  * `GET /api/v1/users` — List users
  * `PUT /api/v1/users/{userId}` — Update contact

### Profiles
* **Scope**: Carrier profiles with their vehicle fleet and merchant business profiles.
* **Aggregates & Entities**: `CarrierProfile` → `Vehicle`, `MerchantProfile`
* **Value Objects**: `CarrierProfileId`, `MerchantProfileId`, `VehicleId`, `Dni`, `Ruc`, `PhoneNumber`, `LicensePlate`, `LoadCapacity`, `VehicleBodyType`
* **Domain Events**: `CarrierProfileCreatedEvent`, `MerchantProfileCreatedEvent`
* **RESTful Endpoints**:
  * `GET|POST /api/v1/carrier-profiles`, `GET|PUT /api/v1/carrier-profiles/{carrierProfileId}`
  * `POST /api/v1/carrier-profiles/{carrierProfileId}/vehicles`, `PUT|DELETE /api/v1/carrier-profiles/{carrierProfileId}/vehicles/{vehicleId}`
  * `GET|POST /api/v1/merchant-profiles`, `GET|PUT /api/v1/merchant-profiles/{merchantProfileId}`

### Matchmaking & Routing
* **Scope**: Core domain. Return routes, freight requests, compatibility evaluation, rate negotiation and match confirmation.
* **Aggregates**: `ReturnRoute`, `FreightRequest`, `MatchProposal`
* **Value Objects**: `TimeWindow`, `Cargo`, `CargoType`, `Detour`, `ERouteStatus`, `ERequestStatus`, `EProposalStatus`, `ENegotiationParty`
* **Domain Service**: `RouteMatchingService`
* **Domain Events**: `ReturnRouteClosedEvent`, `FreightRequestCancelledEvent`, `MatchProposalConfirmedEvent`
* **RESTful Endpoints**:
  * `/api/v1/return-routes` (+ `/{id}/closure`, `/{id}/load-suggestions`)
  * `/api/v1/freight-requests` (+ `/{id}/publication`, `/{id}/cancellation`, `/{id}/carrier-suggestions`)
  * `/api/v1/match-proposals` (+ `/{id}/acceptance`, `/{id}/counteroffers`, `/{id}/rejection`, `/{id}/confirmation`)

### Service Execution & Monitoring
* **Scope**: Shipment lifecycle from match to reception, live location, route deviation and incidents.
* **Aggregates & Entities**: `Shipment` → `Incident`, `ShipmentEvent`
* **Value Objects**: `EShipmentStatus`, `IncidentType`, `EIncidentStatus`, `EPartyRole`
* **Domain Events**: `ShipmentDeviatedEvent`, `ShipmentDeliveredEvent`, `ShipmentClosedEvent`, `IncidentReportedEvent`
* **RESTful Endpoints**:
  * `/api/v1/shipments` (+ `/{id}/pickup`, `/{id}/locations`, `/{id}/delivery`, `/{id}/reception`, `/{id}/cancellation`, `/{id}/incidents`, `/{id}/incidents/{incidentId}/resolution`)

### Payment & Billing
* **Scope**: Subscription plans, card payments through a payment gateway and electronic receipts (boleta / factura).
* **Aggregates**: `PaymentTransaction`, `Receipt`
* **Value Objects**: `SubscriptionPlan`, `PaymentMethod`, `ReceiptType`, `EPaymentStatus`
* **Domain Events**: `PaymentTransactionPaidEvent`, `ReceiptIssuedEvent`
* **RESTful Endpoints**:
  * `/api/v1/payment-transactions`, `/api/v1/receipts`, `/api/v1/subscription-plans`, `/api/v1/users/{userId}/subscription`

### Loyalty & Reputation
* **Scope**: Ratings between carriers and merchants after a delivered shipment and reputation summaries.
* **Aggregates**: `Rating`
* **Value Objects**: `Score`, `RatingTarget`, `ReputationSummary`
* **Domain Events**: `RatingSubmittedEvent`
* **RESTful Endpoints**:
  * `/api/v1/ratings`, `/api/v1/users/{userId}/reputation`

### Shared Kernel
* **Scope**: Common building blocks used by every context.
* **Contents**: `AggregateRoot`, `IEntityId<T>`, `Result<T>`, `Error`, `Money`, `Address`, `GeoLocation`, `LimaDistricts`, `LimaCalendar`, interceptors, pipeline behaviors, `ProblemDetailsFactory` and `GlobalExceptionHandler`.

---

## Technologies Used

* **.NET 10** & **C# 14**: Core runtime and language.
* **ASP.NET Core**: Web API framework.
* **Entity Framework Core**: Relational database ORM (MySQL).
* **MySQL**: Primary database.
* **Cortex.Mediator**: In-process mediator, pipeline behaviors and domain event publishing.
* **Scalar.AspNetCore** & **Microsoft.AspNetCore.OpenApi**: Interactive OpenAPI documentation.
* **BCrypt.Net-Next**: Password hashing.
* **Microsoft.IdentityModel.JsonWebTokens**: JWT token generation and validation.
* **Humanizer**: Table name pluralization for the snake_case naming convention.
* **Microsoft.Extensions.Localization**: English and Spanish localization.

---

## Getting Started

### Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/)
* [MySQL 8](https://dev.mysql.com/downloads/) (local instance or Docker container)
* JetBrains Rider or Visual Studio 2026 / VS Code

### Setup Instructions

1. **Clone the repository:**
   ```bash
   git clone https://github.com/StackRoot-1ASI0730-2620-8084/trazza-platform.git
   cd trazza-platform
   ```

2. **Configure the database** in `Trazza.Platform/appsettings.Development.json`:
   ```json
   "DefaultConnection": "server=localhost;user=root;password=mysql;database=trazza-platform-dev"
   ```

3. **Restore, create the migration and build:**
   ```bash
   dotnet restore
   dotnet tool install --global dotnet-ef
   dotnet ef migrations add InitialCreate --project Trazza.Platform
   dotnet build
   ```

4. **Run the application** (pending migrations are applied on startup):
   ```bash
   dotnet run --project Trazza.Platform
   ```

5. **Explore Interactive API Reference:**
   * Scalar Documentation: [http://localhost:5222/scalar/v1](http://localhost:5222/scalar/v1)
   * OpenAPI Specification: [http://localhost:5222/openapi/v1.json](http://localhost:5222/openapi/v1.json)

6. **Connect the web application** setting `VITE_TRAZZA_PLATFORM_API_URL="http://localhost:5222/api/v1"` in `trazza-webapp`.

---

## Project Structure

```
Trazza.Platform/
├── Iam/                     # IAM Bounded Context
│   ├── Application/         # User Command/Query Services, ACL Facade, Outbound Services
│   ├── Domain/              # User Aggregate, Value Objects, Events, Errors, Repositories
│   ├── Infrastructure/      # UserRepository, TokenService, HashingService, Authorization Middleware
│   └── Interfaces/          # AuthenticationController, UsersController, Resources, ACL contract
├── Profiles/                # Profiles Bounded Context
│   ├── Application/         # Profile Command/Query Services, Event Handlers, ACL Facade
│   ├── Domain/              # CarrierProfile, Vehicle, MerchantProfile, Value Objects
│   ├── Infrastructure/      # Profile Repositories, EF Core configuration
│   └── Interfaces/          # CarrierProfilesController, MerchantProfilesController
├── Matchmaking/             # Matchmaking & Routing Bounded Context
│   ├── Application/         # Route, Request and Proposal Services, Event Handlers, PublicationPolicy
│   ├── Domain/              # ReturnRoute, FreightRequest, MatchProposal, RouteMatchingService
│   ├── Infrastructure/      # Repositories, EF Core configuration
│   └── Interfaces/          # ReturnRoutes, FreightRequests and MatchProposals Controllers
├── Execution/               # Service Execution & Monitoring Bounded Context
│   ├── Application/         # Shipment Services, Event Handlers, ACL Facade
│   ├── Domain/              # Shipment, Incident, ShipmentEvent
│   ├── Infrastructure/      # ShipmentRepository, EF Core configuration
│   └── Interfaces/          # ShipmentsController
├── Billing/                 # Payment & Billing Bounded Context
│   ├── Application/         # Payment and Receipt Services, IPaymentGateway, ACL Facade
│   ├── Domain/              # PaymentTransaction, Receipt, SubscriptionPlan, PaymentMethod
│   ├── Infrastructure/      # Repositories, SimulatedPaymentGateway
│   └── Interfaces/          # PaymentTransactions, Receipts and Subscriptions Controllers
├── Reputation/              # Loyalty & Reputation Bounded Context
│   ├── Application/         # Rating Command/Query Services
│   ├── Domain/              # Rating, Score, RatingTarget, ReputationSummary
│   ├── Infrastructure/      # RatingRepository
│   └── Interfaces/          # RatingsController, UserReputationController
├── Shared/                  # Shared Kernel & Infrastructure
│   ├── Application/         # Result<T>, IEventHandler
│   ├── Domain/              # AggregateRoot, IEntityId, Error, Money, Address, GeoLocation
│   ├── Infrastructure/      # AppDbContext, Pipeline Behaviors, Interceptors, OpenAPI, GlobalExceptionHandler
│   ├── Interfaces/          # ProblemDetailsFactory, shared resources and assemblers
│   └── Resources/           # Common & Error localization files (en, es)
├── Program.cs               # Startup configuration and DI registrations
└── appsettings.json         # Configuration settings
```

---

## Documentation

* **REST API Endpoints**: [`docs/endpoints.md`](docs/endpoints.md)
* **Technical User Stories**: [`docs/user-stories.md`](docs/user-stories.md)
* **Architecture Decision Records (ADRs)**: [`docs/adrs.md`](docs/adrs.md)
* **PlantUML Class Diagram**: [`docs/class-diagram.puml`](docs/class-diagram.puml)
* **Software Architecture DSL (C4)**: [`docs/software-architecture.dsl`](docs/software-architecture.dsl)
* **Study Guide**: [`docs/study-guide.md`](docs/study-guide.md)
* **Study Guide Specification**: [`docs/study-guide-spec.md`](docs/study-guide-spec.md)

---

## Team

**StackRoot** — Web Applications Development (1ASI0730), Universidad Peruana de Ciencias Aplicadas.

| Member | Bounded Context |
|:--|:--|
| Emanuel Renato Checalla Apaza | |
| Fabricio Jofred Lozano Quispe | Payment & Billing |
| Ingrid Melani Medina Merma | |
| Gabriel Augusto Peñaranda Caldas | |
| Rodrigo Matias Vite Celis | Loyalty & Reputation, Shared Kernel |

---

## Contributing & Changelog

* See [`CONTRIBUTING.md`](CONTRIBUTING.md) for contribution guidelines, commit standards, and workflow.
* See [`CHANGELOG.md`](CHANGELOG.md) for version release notes.

---

## License

This project is licensed under the **MIT License**. See [`LICENSE.md`](LICENSE.md) for details.

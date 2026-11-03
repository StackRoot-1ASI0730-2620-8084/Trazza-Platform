# Contributing to Trazza Platform

Thank you for contributing to the **Trazza Platform**! We welcome improvements, bug fixes, and suggestions. Please review this guide to understand our engineering practices and workflow.

---

## 1. Development Principles

We adhere strictly to modern .NET development best practices:
- **C# 14 & .NET 10 Standards:** Primary constructors, collection expressions (`[.. items]`), file-scoped namespaces, pattern matching, records, and nullable reference types enabled.
- **Clean Code without Comments:** Source files contain no comments; intention is expressed through names of types, methods, and variables.
- **Domain-Driven Design (DDD):**
  - Strict bounded contexts (`Iam`, `Profiles`, `Matchmaking`, `Execution`, `Billing`, `Reputation`, `Shared`).
  - Aggregate roots inherit from `AggregateRoot`, protect their invariants, and return `Error` from their behaviors instead of throwing.
  - Strongly-typed identifier record structs (e.g. `UserId`, `ReturnRouteId`, `ShipmentId`) implementing `IEntityId<int>` to prevent primitive obsession.
  - Cross-context communication only through ACL facades (`IIamContextFacade`, `IProfilesContextFacade`, `IBillingContextFacade`, `IExecutionContextFacade`) or domain events.
- **CQRS (Command Query Responsibility Segregation):**
  - Clear separation between Command Services (write side) and Query Services (read side).
  - Cross-cutting concerns managed through Cortex Mediator behaviors (`LoggingCommandBehavior`, `TransactionCommandBehavior`, `ExceptionHandlingCommandBehavior`).
  - Read queries use `.AsNoTracking()` and `.AsSplitQuery()` where collections are included.
- **Functional Result Pattern:**
  - Return `Result<T>` and strongly-typed `Error` records instead of throwing exceptions for expected business errors.
  - Controllers resolve results with `.Match()` through the bounded context `*ActionResultAssembler`.

---

## 2. Conventional Commits

All commits must follow the [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) specification:

```
<type>(<scope>): <short description>

[optional body]

[optional footer(s)]
```

### Commit Types
- `feat`: A new feature or endpoint.
- `fix`: A bug fix.
- `refactor`: Code restructuring without changing external behavior or fixing bugs.
- `perf`: A code change that improves execution performance or reduces allocations.
- `test`: Adding missing tests or correcting existing tests.
- `docs`: Documentation-only changes.
- `chore`: Maintenance tasks, dependency updates, or configuration changes.

### Common Scopes
- `iam`: Identity & Access Management bounded context.
- `profiles`: Carrier and merchant profiles, vehicles.
- `matchmaking`: Return routes, freight requests, match proposals, route matching.
- `execution`: Shipments, tracking, incidents.
- `billing`: Subscription plans, payment transactions, receipts.
- `reputation`: Ratings and reputation summaries.
- `shared`: Shared kernel, pipeline behaviors, or domain abstractions.
- `openapi`: OpenAPI and Scalar documentation configuration.
- `persistence`: Entity Framework Core configurations and repositories.

---

## 3. Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [MySQL 8](https://dev.mysql.com/downloads/) (local instance or Docker container)
- JetBrains Rider or your preferred .NET IDE

### Local Environment Setup
1. Clone the repository:
   ```bash
   git clone https://github.com/StackRoot-1ASI0730-2620-8084/trazza-platform.git
   cd trazza-platform
   ```

2. Start MySQL and update the connection string in `Trazza.Platform/appsettings.Development.json`:
   ```bash
   docker run -d --name trazza-mysql -e MYSQL_ROOT_PASSWORD=mysql -p 3306:3306 mysql:8
   ```

3. Create the database migration, build and run the solution:
   ```bash
   dotnet tool install --global dotnet-ef
   dotnet ef migrations add InitialCreate --project Trazza.Platform
   dotnet build
   dotnet run --project Trazza.Platform
   ```

4. Access the interactive API reference:
   - Scalar UI: `http://localhost:5222/scalar/v1`
   - OpenAPI document: `http://localhost:5222/openapi/v1.json`

---

## 4. Code Style & Quality Checklist

Before submitting a Pull Request, please ensure:
1. **Build Verification:** Solution compiles without warnings or errors (`dotnet build`).
2. **Architecture Conformity:**
   - New aggregates and entities are placed in the appropriate bounded context.
   - Aggregate roots manage their domain events using `AddDomainEvent`.
   - Repositories utilize `StronglyTypedIdConverter` for strongly-typed identifier keys.
   - REST controllers map domain results via `.Match()` in the context `*ActionResultAssembler`.
   - New error codes are added to the context `*Errors` catalog and translated in `Shared/Resources/Errors/ErrorMessages.resx` and `ErrorMessages.es.resx`.
3. **Documentation:**
   - Update `docs/user-stories.md` and `docs/endpoints.md` if adding or changing REST endpoints.
   - Update `docs/class-diagram.puml` if modifying entities, interfaces, or relationships.
   - Add Architecture Decision Records in `docs/adrs.md` for major technical decisions.
   - Update `CHANGELOG.md` under `[Unreleased]` or target version.

---

## 5. Pull Request Process

1. Create a feature branch from `develop`:
   ```bash
   git checkout develop
   git checkout -b feature/your-feature-name
   ```
2. Commit your changes following conventional commit syntax.
3. Push to your branch and open a Pull Request to `develop` with a clear description of changes, motivation, and verification steps.
4. `develop` is merged into `main` for each release.

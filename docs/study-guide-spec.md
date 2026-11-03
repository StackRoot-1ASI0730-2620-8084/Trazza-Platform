# Study Guide Specification

## Using AI to Analyze and Learn from a Codebase

Use the following prompt to create a study guide specification for analyzing a Codebase:

---

```
Given I am a Software Engineering Student and this Codebase is an example of a RESTful API, part of the Web 
Applications course I am enrolled in, so analyze the codebase and help me understand the approaches, patterns, 
Framework and language features applied, so I can apply them in my own projects. Based on this, create a 
Study Guide (docs/study-guide.md), including detailed explanations and references for each relevant topic, 
so I as an student can use it as a source for learning and developing my skills.
```

---

A more specific prompt can be created by adding the following information:

```
Given I am a Software Engineering Student and this Codebase is an example of a production-quality application, 
analyze the entire codebase and help me understand the approaches, patterns, frameworks, and language features applied, 
so I can apply them in my own projects.

Based on this analysis, create a comprehensive Study Guide document at `docs/study-guide.md`.

## Role & Audience

Write the guide for a software engineering student who wants to learn from this codebase as a reference implementation. 
Each topic should be explained at a level that helps the student understand not just what the code does, but why 
it was designed that way and how they can replicate the pattern.

## Document Structure

Organize the study guide into the following sections, adjusting the technical content to match what the codebase 
actually uses (omit sections that don't apply, add sections for patterns or features not listed here but present 
in the codebase):

### 1. Overview
- Summarize the project: what it builds, the technology stack (framework version, language version, database, etc.), 
  and the key architectural approaches.
- If the codebase uses modular decomposition (bounded contexts, modules, microservices, feature folders), describe 
  the modules and their responsibilities.

### 2. Architecture Patterns
For each architectural pattern found in the codebase, provide a subsection that includes:
- **Concept**: A clear definition of the pattern and when to use it.
- **In the Codebase**: A code snippet extracted from the actual codebase showing the pattern in use, with the 
  relevant file paths listed.
- **Key Files**: Paths to the files that best demonstrate this pattern.
- **Why It Matters**: A practical explanation of the benefit this pattern provides.

Patterns to look for (include only those present): Clean Architecture, Layered Architecture, Hexagonal/Ports & 
Adapters, Repository, Unit of Work, Dependency Injection, Middleware, Assembler, Service Layer, Facade, 
Strategy, Observer, Builder, Factory, Singleton, Adapter, Decorator, and others.

### 3. Domain-Driven Design (DDD)
If the codebase applies DDD, cover the concepts found:
- **Bounded Contexts**: How the domain is partitioned and why.
- **Aggregate Roots**: How aggregates are defined and protected.
- **Value Objects**: Immutable, identity-less objects and their validation.
- **Strongly-Typed IDs**: Type-safe identifiers instead of primitives.
- **Domain Events**: How significant domain occurrences are raised and handled.
- **Anti-Corruption Layer**: How external models are isolated.
- **Entities, Domain Services, Specifications**, and other DDD building blocks present.

For each concept, follow the same format: Concept, In the Codebase (with snippets and file paths), Key Files, 
Why It Matters.

### 4. CQRS Pattern
If commands and queries are separated, explain:
- How commands (write operations) and queries (read operations) are structured.
- How command and query objects are defined (records, classes, etc.).
- The benefits of this separation in the context of this codebase.

### 5. Framework Features
Cover the main framework and its features used in the codebase. For web applications, this typically includes:
- Web framework setup (hosting, routing, controllers/endpoints).
- Configuration system (settings, environment variables, secrets).
- Localization/internationalization if present.
- API documentation (OpenAPI/Swagger or similar).
- Background workers, scheduling, or messaging if applicable.
- Logging, monitoring, or observability features.

### 6. Language Features
Identify and explain the modern language features used throughout the codebase. For each feature:
- **Concept**: What the feature is.
- **In the Codebase**: A real code snippet demonstrating it.
- **Key Files**: Where to find more examples.
- **Why It Matters**: Why this feature improves the code.

Examples: records, primary constructors, pattern matching, nullable reference types, file-scoped namespaces, 
collection expressions, top-level statements, pattern matching in switch expressions, async/await, extension 
methods, expression-bodied members, and others.

### 7. Data Access / ORM
If an ORM or data access layer is used, cover:
- Context/connection configuration.
- Entity mapping (Fluent API, data annotations, or decorators).
- Value converters or type mappings.
- Interceptors, hooks, or lifecycle events.
- Query optimization techniques (no-tracking, compiled queries, etc.).
- Repository implementation over the ORM.
- Migrations or schema management.

### 8. Security & Authentication
If security features are present, cover:
- Authentication mechanism (JWT, OAuth2, session-based, API keys, etc.).
- Password hashing or credential storage.
- Authorization (roles, policies, claims, middleware).
- Input validation and sanitization.
- CORS, HTTPS, or other transport security.

### 9. API Design
For API-oriented projects, cover:
- RESTful endpoint design (routing conventions, HTTP methods, status codes).
- Request/response resource (DTO) design.
- Error handling and standard error formats (Problem Details, custom formats).
- Result pattern or similar explicit success/failure handling.
- Versioning, pagination, filtering, or sorting if present.

### 10. Testing & Quality
Cover the testing and code quality approaches:
- Test structure and organization.
- Mediator pipeline behaviors, middleware, or aspect-oriented patterns for cross-cutting concerns.
- Code organization and folder structure conventions.
- Linting, formatting, or static analysis tools.
- CI/CD quality gates if visible.

### 11. DevOps & Deployment
If deployment artifacts are present, cover:
- Containerization (Docker, multi-stage builds).
- Orchestration (Kubernetes, Docker Compose).
- CI/CD pipelines (GitHub Actions, GitLab CI, etc.).
- Environment configuration and secrets management.
- Database migration strategy.

### 12. References
Provide a curated learning section:

#### Official Documentation
- Links to the official documentation for the main frameworks and libraries used, each with a short description.

#### Books
- List authoritative books relevant to the patterns and techniques used in the codebase. Include author, year, 
  and a one-sentence description. Prioritize well-known, current references. Include books on:
  - Domain-Driven Design (e.g., Evans' "Blue Book", Vernon's "Red Book" and "Distilled")
  - Enterprise application patterns (e.g., Martin Fowler's "Patterns of Enterprise Application Architecture")
  - Architecture principles (e.g., Clean Architecture)
  - Language-specific deep dives (e.g., current edition of the language reference)
  - Framework-specific practical guides

#### Online Resources
- Curated links to high-quality online learning resources, tutorials, pattern catalogs, and community sites. 
  Include descriptions and note if content is archival.

#### Key Concepts to Research
- A numbered list of 6–10 broad topic areas the student should research further, drawn from what the codebase demonstrates.

#### Exercises
- A numbered list of 6–10 practical exercises that let the student apply what they learned by extending or 
  modifying the codebase. Exercises should range from straightforward (add a new endpoint) to challenging 
  (implement a new pattern or refactor a module).

## Writing Rules

1. **Extract real code snippets** from the codebase — do not invent examples. Use actual file paths.
2. **Keep explanations concise** — each subsection should be scannable: concept, example, files, why it matters.
3. **Use Markdown formatting** — headers, code blocks with language tags, bold for emphasis, lists for structure.
4. **Include a Table of Contents** at the top with anchor links to each section.
5. **Adapt to the codebase** — if a section doesn't apply (e.g., no Dockerfile), omit it. If the codebase uses 
   patterns not listed above, add them.
6. **Validate references** — ensure all book titles, author names, and URLs are accurate and current.
7. **Write for learners** — assume the reader knows basic programming but may not know the advanced patterns 
   demonstrated in this codebase.
8. **Order sections logically** — start with big-picture architecture, then domain concepts, then framework 
   features, then language features, then operational concerns, and end with references.

## Output

Create the file `docs/study-guide.md` with the complete study guide.
```

---

## How to Use These Prompts

1. **Copy** one of the prompt blocks above (between the triple backticks).
2. **Open** your AI coding assistant (Junie, Cursor, Copilot, etc.) in the target repository.
3. **Paste** the prompt and run it.
4. **Review** the generated `docs/study-guide.md` and adjust any sections that don't accurately reflect the codebase.

## Tips for Best Results

- Run the prompt in a repository that has a meaningful codebase (not a blank starter project).
- If the repository is very large, you may want to first ask the assistant to identify the key modules and
  patterns before generating the full guide.
- After generation, ask the assistant to validate the references section to ensure URLs and book information
  are current.
- For team use, have each team member review the sections related to their area of expertise.
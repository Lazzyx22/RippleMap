# RippleMap

Explain what depends on a resource, and why a change to it may matter.

RippleMap is a planned dependency-impact explorer for small software teams. Import a JSON map of applications, services, databases, and external APIs; select a resource; inspect its direct and indirect dependents with explanatory paths.

```text
Web app ----> Checkout ----> Payment provider
Mobile app -> Checkout ----> Orders database
Refunds ------------------> Payment provider

A -> B means A depends on B.
```

Selecting the payment provider should explain four potentially affected resources: Checkout, Refunds, Web app, and Mobile app. Orders database is excluded because it is another dependency of Checkout, not a dependent of the payment provider.

## Status: 30 October 2026

The repository contains an ASP.NET Core starter and design documents. Business endpoints, Neo4j access, the browser interface, and automated application tests are planned work.

The weather sample controller, model, and HTTP request file have been removed. The API foundation builds successfully; business endpoints are not yet implemented.

## First useful release

- Validate complete dependency maps before persistence.
- Import immutable snapshots with safe repeated requests.
- List snapshots and search resources.
- Explain incoming dependency paths with visible depth and result limits.
- Export JSON that can be imported again.
- Run locally with persistent storage and verified failure behavior.

The answer describes potential impact within the supplied map. It does not establish that an outage will happen or that the map is complete.

## Documentation

| Guide | Purpose |
| --- | --- |
| [00 - Start here](RippleMap.Api/Docs/00-START-HERE.md) | Current state, scope, priorities, completion gates |
| [01 - Research and database decisions](RippleMap.Api/Docs/01-RESEARCH-AND-DATABASES.md) | Is it needed? Why choose a graph? |
| [02 - Architecture and flow](RippleMap.Api/Docs/02-PROJECT-FLOW.md) | Clean Architecture, layout, request flows, operations |
| [03 - Data and API contract](RippleMap.Api/Docs/03-DATA-AND-API.md) | Authoritative prototype schema, HTTP contracts, graph semantics |
| [04 - Implementation plan](RippleMap.Api/Docs/04-SEVEN-DAY-PLAN.md) | Beginner-to-intermediate milestones and acceptance criteria |
| [05 - How it works](RippleMap.Api/Docs/05-HOW-IT-WORKS.md) | Concepts, examples, debugging, review questions |
| [06 - Quality and release gates](RippleMap.Api/Docs/06-QUALITY-AND-ROADMAP.md) | Tests, contribution workflow, release evidence |
| [07 - RippleMap explained](RippleMap.Api/Docs/07-RIPPLEMAP-EXPLAINED.md) | Detailed product explanation and developer handoff |
| [08 - Advanced features](RippleMap.Api/Docs/08-ADVANCED-FEATURES.md) | Optional features, prerequisites, costs, adoption triggers |

Read 07 for the product story, then 00, 02, 03, and 04 to implement it. The advanced backlog does not expand the prototype contract.

## Development starting point

From the directory containing RippleMap.slnx:

```powershell
dotnet --info
dotnet restore RippleMap.slnx
dotnet build RippleMap.slnx
```

The existing project targets net10.0. After a successful build:

```powershell
dotnet run --project RippleMap.Api/RippleMap.Api.csproj --launch-profile https
```

The checked-in profile uses https://localhost:7025 and http://localhost:5104. This runs the starter application, not the planned RippleMap UI. Development OpenAPI generation is configured; an interactive documentation UI is a separate choice.

Neo4j setup, a pinned driver, health checks, a sample file, tests, and reproducible database startup still need implementation. Follow the plan rather than assuming these assets exist.

## Engineering approach

Use one API and one database. Start with folders separating HTTP, application use cases, domain rules, and Neo4j access. Extract class libraries when compiler-enforced boundaries become useful. Prefer platform features and explicit code over generic frameworks.

The prototype is local and single-user. A shared pilot requires identity, authorization, access-control tests, recovery verification, and operational ownership before exposing real company maps.

No adoption, performance benefit, production readiness, or project-name availability is established. Choose a license before representing the source as reusable open source.

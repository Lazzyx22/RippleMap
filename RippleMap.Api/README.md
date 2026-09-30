# RippleMap

Understand what depends on what—and what could be affected when a service changes.

RippleMap is a dependency-impact explorer for small software teams. It is designed to import a JSON map of applications, services, databases, and external APIs, then explain dependency paths using Neo4j and Cypher.

**Status:** Early development. The documentation defines the planned behaviour; features are not yet verified as implemented.

## Planned prototype

- Validate and import dependency maps as immutable snapshots.
- Search resources and trace direct and indirect dependents.
- Show the paths behind each potential impact.
- Export maps as JSON.

Results depend on the accuracy of the imported map; they describe possible impact, not a guaranteed outage.

## Stack and setup

C# · ASP.NET Core (.NET 10) · Neo4j Community · Cypher · HTML/CSS/JavaScript · xUnit

Create an **ASP.NET Core Web API** project named `RippleMap.Api`, with **Controllers**, **OpenAPI**, and **HTTPS** enabled. Leave **container support** unchecked initially. Follow the setup steps in the seven-day plan below.

## Documentation

Keep this README and the following Markdown files together in the same directory so these relative links work on GitHub.

| Guide | Contents |
| --- | --- |
| [Start here](00-START-HERE.md) | Purpose, scope, and definition of done |
| [Research and databases](01-RESEARCH-AND-DATABASES.md) | Problem research, alternatives, and database choices |
| [Project flow](02-PROJECT-FLOW.md) | Architecture, user journeys, and failure paths |
| [Data and API](03-DATA-AND-API.md) | Sample JSON, graph model, endpoints, and Cypher |
| [Seven-day plan](04-SEVEN-DAY-PLAN.md) | Visual Studio setup, daily tasks, and time estimates |
| [How it works](05-HOW-IT-WORKS.md) | Beginner explanations and worked examples |
| [Quality and roadmap](06-QUALITY-AND-ROADMAP.md) | Tests, design decisions, and future modules |

## Build target

The first milestone is a local, single-user prototype: **44 planned hours plus 6 hours of reserve**. Shared deployment, authentication, integrations, and custom templates belong to later milestones.

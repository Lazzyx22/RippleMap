# RippleMap

**See what depends on what. Understand the possible impact of a change.**

Project brief and reading guide · 30 September 2026 · Planned version: 0.1

RippleMap is a proposed open-source dependency-impact explorer for small software teams. A team describes its applications, services, databases, external APIs, and dependencies. RippleMap stores that description as a graph and explains which resources could be affected by a selected dependency.

This package contains design documents and exercises, not an implemented application. Commands and interfaces described as planned become available only when you implement them. The name is a working project name; public repository, package, domain, and trademark availability have not been checked.

## Start with one useful question

“If this payment provider changes or becomes unavailable, which parts of our system should we inspect, and why?”

A resource is anything in the map: an application, service, database, or external API. A dependency is a directed statement: resource A relies on resource B. Impact analysis follows those statements backwards from the selected target to find the resources that rely on it.

The answer is potential impact based on recorded data. An optional dependency, a cache, or a fallback can prevent a real outage. Missing or stale relationships can hide genuine impact. The product must explain its evidence and limits.

## What the one-week version does

1. Accept a small JSON dependency map.
2. Validate every resource and relationship before writing.
3. Store a complete, immutable dataset in Neo4j.
4. Let a user select a dataset and search its resources.
5. Show recorded dependency paths leading to a selected target.
6. Export the dataset as JSON.
7. Run locally with documented setup and meaningful tests.

The core interface is a searchable list and readable path results. A polished interactive graph is a stretch feature. This choice keeps the useful answer achievable in one week.

## The target and its limits

The plan allocates **44 hours of implementation, learning, and verification, plus a 6-hour reserve: about 50 focused hours across seven days**. This is an aggressive prototype target, not a promise of completion. It assumes you can already run and edit basic C# and will learn graph concepts during the work.

At 2 hours per day, the same 50-hour scope takes about 25 working days. If “one week” is fixed, reduce the feature set rather than skipping correctness checks.

By the end of the week, the definition of done is:

- [ ] The six-resource example imports successfully.
- [ ] Importing the same dataset and content twice does not create duplicates.
- [ ] A conflicting import leaves the existing dataset unchanged.
- [ ] Invalid references and duplicate IDs produce useful validation errors.
- [ ] Selecting the payment provider explains four affected resources in the fixture.
- [ ] Cyclic graphs terminate safely within documented limits.
- [ ] Data survives an application restart.
- [ ] Resource names are rendered as text, not executable HTML.
- [ ] A fresh local setup can follow the README and reproduce the demo.
- [ ] The tests, known limits, and unimplemented features are documented honestly.

The prototype is local and single-user. It does not include authentication or authorisation, so it is not a shared company deployment. Bind its web endpoint and database ports to loopback. The later pilot adds access control and operational safeguards before exposing real company data.

## The selected stack

| Concern | Selection | Reason |
| --- | --- | --- |
| Backend | C# and ASP.NET Core on .NET 10 | One API application with clear boundaries |
| Database | Neo4j Community | Explicit graph relationships and Cypher queries |
| Database access | Official Neo4j.Driver package | Direct, documented .NET integration |
| Interface | Static HTML, CSS, and small JavaScript files served by the API | One application to run; little build-tool overhead |
| Testing | xUnit and a real isolated Neo4j test database | Verify both application rules and actual graph queries |
| Local database setup | A pinned Neo4j Community container, or Neo4j Desktop | Choose one documented path during setup |
| Packaging | Reproducible local startup; application container if time remains | Make the demo reproducible before adding hosting |

This prototype has one application and one database. Its logical modules are not separate deployed microservices. Keep the database password on the server; the browser talks only to the application.

## Read these files in order

| File | What you get |
| --- | --- |
| [01-RESEARCH-AND-DATABASES.md](01-RESEARCH-AND-DATABASES.md) | Reported problems, existing alternatives, database comparison, and why this idea is a hypothesis worth testing |
| [02-PROJECT-FLOW.md](02-PROJECT-FLOW.md) | User journeys, request flow, architecture, and failure paths |
| [03-DATA-AND-API.md](03-DATA-AND-API.md) | Canonical sample input, graph model, validation, API contracts, and Cypher examples |
| [04-SEVEN-DAY-PLAN.md](04-SEVEN-DAY-PLAN.md) | Daily tasks, time limits, expected results, and scope cuts |
| [05-HOW-IT-WORKS.md](05-HOW-IT-WORKS.md) | Beginner explanations, component responsibilities, and a complete worked example |
| [06-QUALITY-AND-ROADMAP.md](06-QUALITY-AND-ROADMAP.md) | Tests, quality standards, all nine original goals, and estimates for later modules |

If two documents appear inconsistent, the data/API document defines the prototype's contract; the seven-day document defines the timebox. Resolve contradictions before implementation rather than silently changing behaviour.

## How to use this as a learning project

For every task: explain the desired behaviour, predict a result, write a small change, run it, inspect one failure, and then test it. Use AI for explanations, hints, and review. If AI writes code, explain its inputs, outputs, dependencies, and failure behaviour before accepting it.

A useful daily note contains three things: what works, what failed, and what you now understand. A feature is complete when another person can reproduce its behaviour, not when the code merely looks finished.

The initial repository can contain two projects, RippleMap.Api and RippleMap.Tests, plus docs and sample fixtures. Add folders inside the API for contracts, features, and infrastructure. Extract separate libraries only when that makes the code easier to change.

This project replaces the earlier inventory example as the active product direction. An API generator or Roslyn extension is not a week-one dependency.


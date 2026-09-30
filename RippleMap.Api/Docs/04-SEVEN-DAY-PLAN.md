# RippleMap: seven-day prototype plan

**Budget: 44 planned hours plus a 6-hour reserve, approximately 50 focused hours.**

The goal is the local version 0.1 defined in [00-START-HERE.md](00-START-HERE.md). It is a stretch target while learning. AI can help with explanations and review, but it does not remove the need to understand and debug database behaviour.

A week at 2 hours per day provides 14 hours, not 50. In that case, finish setup, validation, and one query first, then continue the remaining plan. Keep the deadline flexible if learning a prerequisite takes longer than expected.

## Daily schedule

| Day | Focus | Hours | Evidence of completion |
| --- | --- | ---: | --- |
| 1 | Setup, vocabulary, sample graph | 6 | API starts; C# reaches Neo4j; sample graph can be inspected |
| 2 | Input contract and validation | 6 | Valid fixture accepted by validator; invalid fixtures explain errors |
| 3 | Transactional import and dataset list | 7 | Create, unchanged, conflict, and rollback behaviours work |
| 4 | Impact query and result contract | 7 | Expected direct/indirect paths; cycles and limits tested |
| 5 | Minimal browser interface and export | 6 | Import, select, search, inspect, and export from browser |
| 6 | Integration tests and failure handling | 7 | Restart persistence, failure cases, and dataset separation verified |
| 7 | Fresh setup, documentation, demo | 5 | Another clean setup reproduces the acceptance checklist |
| Reserve | Unexpected setup, debugging, and learning | 6 | Used where actual evidence shows a problem |
| **Total** | | **50** | |

Testing is included throughout. Day six is for cross-component and operational cases, not the first day on which we test anything.

## Day 1: know what is running

Spend roughly one hour understanding the problem and tracing the six-resource example. Spend two hours on environment setup, two on the first connection and sample graph, and one on explaining the request/database boundary.

Install or verify:

- Visual Studio 2026 with ASP.NET and web development.
- .NET 10 SDK.
- Git.
- A local Neo4j Community instance, using one chosen route.
- A compatible stable Neo4j.Driver NuGet package.

Microsoft documents .NET 10 targeting with Visual Studio 2026 18.0 or later: [installation and version guidance](https://learn.microsoft.com/en-us/dotnet/core/install/windows). Its [.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core) identifies .NET 10 as LTS.

If you already have Visual Studio, use Installer > Modify to add the workload. In the IDE, Help > About shows the installed version.

Use these templates:

| Template | Project name | Settings |
| --- | --- | --- |
| ASP.NET Core Web API | RippleMap.Api | C#, .NET 10, controllers, OpenAPI, local HTTPS |
| xUnit Test Project | RippleMap.Tests | C#, modern .NET; reference the API project |

Leave application container support off initially unless it is already familiar. A separate Class Library is not required in week one.

A container engine is a separate prerequisite if you choose Docker for Neo4j. Otherwise use Neo4j Desktop. Record the exact database and driver versions that you verify together. Pin them; avoid an unrecorded latest image.

The official driver installation command, run inside the API project directory, is:

```powershell
dotnet add package Neo4j.Driver
```

Then record the resolved package version in the project file and keep it stable during the sprint. See the [driver documentation](https://neo4j.com/docs/dotnet-manual/current/).

For a container setup, bind exposed database ports to 127.0.0.1, enable authentication, and use a persistent data volume. The database's Bolt connection is typically on port 7687; its browser tooling is typically on 7474. Follow the current [official container setup](https://neo4j.com/docs/operations-manual/current/docker/introduction/) for the selected version.

Store the database password in development secrets or an uncommitted environment file. Commit a configuration example with placeholders. Check that secrets and generated build output are excluded from Git.

Build /health/live and /health/ready. The first proves the API process responds. The second checks database connectivity. They answer different questions.

**Explain before moving on:** What is the browser? What is the server? What is a driver? Where does the password live? Why does an edge have a direction?

## Day 2: build the contract before persistence

Use the sample in [03-DATA-AND-API.md](03-DATA-AND-API.md). Write request DTOs, a validator, and deterministic normalisation.

Start with the valid fixture. Then make one change at a time:

- Duplicate checkout's ID.
- Refer to a missing resource.
- Add a self-dependency.
- Supply an unsupported kind.
- Remove a required field.
- Exceed a documented count or size limit.
- Reorder arrays without changing their meaning.

Validation must finish before any database write. Add tests for useful field paths such as dependencies[2].to.

The DTO represents data crossing an API boundary. It is not automatically a trusted domain object. The validator turns untrusted input into a valid map the importer can safely process.

**Explain before moving on:** Why is a map with a missing endpoint invalid? Why do IDs matter more than display names? Why should array order not affect the import hash?

## Day 3: import safely

Implement ImportService and the graph-store boundary. Create uniqueness constraints. Add dataset listing.

Use one write transaction for the complete graph. Return success only after commit. Handle identical imports, conflicting content, unavailable Neo4j, and concurrent attempts to create the same dataset ID.

Do not add file uploads to disk, a background queue, or a second database. The small JSON request can be processed synchronously.

Tests to finish today:

1. First import creates six resources and five relationships.
2. The second identical import returns unchanged and counts remain stable.
3. Reordered equivalent input also returns unchanged.
4. Different content under demo-v1 returns conflict.
5. A simulated failure during the transaction leaves no partial dataset.
6. A second dataset can contain a checkout ID without colliding with demo-v1.

**Explain before moving on:** What is a transaction? Why are constraints useful even with application validation? What happens if the server commits but the response never reaches the browser?

## Day 4: make the useful answer correct

Implement the impact endpoint. Begin with one direct edge, then a two-edge path, then the complete fixture.

Check incoming direction, maximum depth, missing targets, cycles, optional-dependency metadata, path limits, and timeout handling. Use parameterised values.

For payment-provider:

- Depth 1: checkout and refunds.
- Depth 2 or greater: checkout, refunds, web-app, and mobile-app.
- orders-db is excluded.

Return path explanations rather than only a count. Make the depth and result cap visible in the contract.

**Explain before moving on:** Why is orders-db excluded? What is the difference between a dependency and a dependent? Why does LIMIT alone not guarantee a cheap traversal?

## Day 5: finish the smallest useful interface

Build one local page with:

- File picker and import result.
- Dataset selector.
- Resource search.
- Selected resource details.
- Maximum-depth control.
- A table of affected resources and explanatory paths.
- Export button.
- Visible error and loading states.

Render resource names with textContent or a template system's normal escaping, not raw innerHTML built from input. Keep the interface same-origin with the API.

A graph visualisation is optional. The path table is the required explanation. If a graph layout library costs several hours, defer it and keep the useful result.

Export should produce a map that passes the same import validator. Test the download by importing it into an isolated fresh instance.

**Explain before moving on:** What does fetch do? Why can the browser not hold the database password? Why is a successful-looking button insufficient evidence that export works?

## Day 6: test the actual system

Run the API against an isolated Neo4j instance using synthetic fixtures. Test application behaviour through HTTP as well as repository queries.

Exercise database unavailability, restart persistence, conflict handling, invalid body size, missing IDs, result caps, and a cycle. Verify one dataset cannot traverse into another because of an incorrect query.

Use a request identifier in logs. Do not log the imported body, database password, or full connection credentials. Distinguish useful operational diagnostics from sensitive data.

Review the whole flow in the debugger. Remove unused abstractions and dead code. Resolve warnings that indicate actual mistakes. Document remaining limits instead of hiding failures.

**Explain before moving on:** Which tests need a real database? What does a passing unit test fail to prove? How do you tell a query timeout from a legitimate empty result?

## Day 7: reproduce and demonstrate

Start from a clean checkout or a second isolated folder. Follow only the README. Restore dependencies, configure the database, run tests, start the app, and reproduce the sample.

Record a short demonstration:

1. Import the sample.
2. Select payment-provider.
3. Explain the four paths.
4. Change the depth to 1.
5. Repeat the same import.
6. Try conflicting content under the same ID.
7. Export the map.

Add setup instructions, architecture links, the supported limits, and contribution guidance. Choose a project license deliberately; public source without a license is not a clear open-source permission grant.

Use a release checklist and tag only an actually working state. A hosted company deployment is outside this sprint.

## Scope cuts if time runs short

Cut features in this order:

1. Animation and graph-layout polish.
2. Extra export formats.
3. Application container packaging; keep the documented local API plus database route.
4. A separate search box; a selectable list is enough for the small fixture.
5. Additional demonstration datasets.

Preserve validation, transactional import, dataset isolation, correct traversal direction, depth limits, useful errors, and tests of those behaviours.

If those core behaviours are not ready by day seven, call the result a work-in-progress prototype. Extend the schedule or cut the deliverable to one correct end-to-end flow.

## The daily learning routine

Use roughly 20% of each session to understand and explain, 60% to implement and debug, and 20% to verify and record what changed. These proportions are guidance, not a timer-based rule.

When asking AI for help, include the smallest relevant code, expected behaviour, actual behaviour, and exact error. Ask it to explain the cause and propose a minimal change. After applying a suggestion, rerun the behaviour yourself and explain why it now works.

Do not measure progress by lines of generated code. Measure it by acceptance criteria you can demonstrate.


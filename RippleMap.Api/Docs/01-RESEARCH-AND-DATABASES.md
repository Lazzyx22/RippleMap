# RippleMap: research and database decision

Research checked on 30 September 2026. This is a shortlist based on public reports and current project documentation, not a representative market survey. Company size is not established for every report. Proposed product advantages are hypotheses.

## The problems investigated

| Candidate | Observed pain | Plausible initial product | Graph fit |
| --- | --- | --- | --- |
| Dependency-impact explorer | Finding indirect dependencies and tracking effects across services | Explain recorded impact paths, ownership, and freshness | Strong: traversing relationships is the main operation |
| Webhook recovery tool | A failed notification may need retries, inspection, and replay | Durable receipt and visible recovery for one integration | Weak initially: a relational event store is sufficient |
| Document-request tracker | Repeated follow-ups and incomplete client submissions | Clear document checklists, upload links, and outstanding-item tracking | Weak initially: ordinary records and file storage fit |
| Spreadsheet import repair | Mapping and validation errors make imports difficult to fix | Explain errors by row and column, preview corrected data | Limited for initial scope |

## Evidence for dependency exploration

A developer in a recent discussion described a small team operating roughly 15–20 services, where changes to shared code and contracts require changes across multiple repositories and gateways. This is one first-person report; it does not establish how widespread the problem is.

Source: [software-architecture discussion](https://www.reddit.com/r/softwarearchitecture/comments/1u8j4ey/microservices_have_probably_wasted_more/).

Dependency-Track issue 3353 requests searching for a component and opening the paths through which it appears in the dependency graph, instead of manually expanding nodes. The issue was opened in January 2024 and was displayed as open during this research. It is a specific usability request, not evidence that the entire product lacks impact analysis.

Source: [Dependency-Track issue 3353](https://github.com/DependencyTrack/dependency-track/issues/3353).

A Backstage request describes modelling releases and attaching version-related information to relationships. That issue is closed. We use it as evidence of a real modelling need, not an unresolved feature claim.

Source: [Backstage issue 24887](https://github.com/backstage/backstage/issues/24887).

## Existing solutions must be taken seriously

[Backstage](https://backstage.io/docs/features/software-catalog/) already offers a software catalog with ownership metadata. [Dependency-Track](https://docs.dependencytrack.org/) already tracks component usage and affected applications. GitHub also exposes dependency graphs and dependency paths.

RippleMap should not claim to have invented dependency graphs. Its proposed advantage is a smaller workflow: import a modest system description, select a resource, see understandable paths, and inspect how current the supporting relationships are.

This advantage is unproven. If existing products already solve a target team's problem comfortably, contributing a focused improvement upstream may be more useful than building another application.

Package dependencies and runtime service dependencies are different data. A package manifest does not tell us that a deployed service calls a particular payment provider. The prototype uses explicit declarations and labels them accordingly.

## What the other research revealed

GitHub documents that failed webhook deliveries are not automatically redelivered. Recovery can be manual or implemented in code. [GitHub's delivery-recovery documentation](https://docs.github.com/en/webhooks/using-webhooks/handling-failed-webhook-deliveries) establishes a concrete integration problem.

[Svix](https://github.com/svix/svix-webhooks) already supplies open-source webhook infrastructure. A new tool would need a focused advantage, not simply a queue and retry button.

A solo bookkeeper described repeated document chasing across around 22 clients. Another thread describes both manual reminders and ignored automated reminders. These reports suggest that unclear requests and client behaviour are part of the problem; adding reminders alone may not solve it.

Sources: [bookkeeping discussion](https://www.reddit.com/r/Accounting/comments/1s715ta/how_do_you_get_clients_to_actually_send_you_what/) and [tax-practice discussion](https://www.reddit.com/r/tax/comments/1tti1n8/how_do_you_handle_chasing_clients_for_documents/).

[Nextcloud's file-drop feature](https://docs.nextcloud.com/server/latest/user_manual/en/files/file_drop.html) already handles anonymous uploads. A document tracker would need to improve request completion, review, or visibility beyond file upload itself.

An older Shopify community discussion describes import errors that are hard to locate in a spreadsheet. This is historical evidence of friction, not a current claim about Shopify's implementation. [Impler](https://docs.impler.io/overview) is an existing data-import platform, so that space also has established alternatives.

Source: [Shopify import discussion](https://community.shopify.com/t/why-does-csv-product-import-to-pos-system-keep-failing/17498/2).

## Validate the proposed advantage

Before expanding the project, ask a few small teams to demonstrate a recent dependency question:

1. What changed, or what was unavailable?
2. How did you identify dependent systems?
3. Which relationships were missing or uncertain?
4. Where is ownership recorded?
5. What existing tool did you try?
6. Would maintaining an explicit map be acceptable?
7. Could we test with a synthetic or sanitised version of that map?

Do not equate “that sounds interesting” with adoption. Ask someone to try a specific task. A useful test is whether the user can find and explain an indirect dependency with less confusion than in their current workflow.

The seven-day sprint proves a small implementation. It does not complete this validation work or establish product-market fit.

## Graph databases, explained

A graph contains nodes, directed relationships, and properties. Cypher is a language for querying a property graph. It is not a database product. GraphQL is a separate API query technology and is not required here.

A relational database could solve this problem with suitable schema design and recursive queries. We choose a graph because traversing and explaining relationships is central to this product and graph modelling is an explicit learning goal. A graph database is not automatically faster for every query.

## Options reviewed

| Database | Language/design | Relevant finding | Decision |
| --- | --- | --- | --- |
| Neo4j Community | Property graph and Cypher | Official .NET driver; Community source is GPL-3.0 | Selected for prototype |
| Memgraph | Cypher-compatible graph database with an in-memory emphasis | Community currently uses BSL licensing | Possible later comparison; do not assume full Cypher interchangeability |
| LadybugDB | Embedded graph database with Cypher | MIT license; README lists several bindings but not C# | Interesting for a local explorer; integration experiment needed |
| SurrealDB | Document/graph and other models; primarily SurrealQL | Release page listed 3.3.0 on 24 September 2026; core uses BSL 1.1 | Interesting multi-model alternative; not the chosen Cypher workflow |

Neo4j is established, not a newly invented database. “New” here includes a database model new to the learner. If exploring a newly released system becomes the primary goal, allocate a separate experiment instead of silently changing the product architecture.

Sources:

- [Neo4j .NET driver](https://neo4j.com/docs/dotnet-manual/current/) and [Community repository](https://github.com/neo4j/neo4j).
- [Memgraph repository and licensing](https://github.com/memgraph/memgraph).
- [LadybugDB repository, design, and bindings](https://github.com/LadybugDB/ladybug).
- [SurrealDB releases](https://surrealdb.com/releases), [SurrealQL](https://surrealdb.com/docs/reference/query-language), and [core licensing](https://github.com/surrealdb/surrealdb).
- [FalkorDB documentation](https://docs.falkordb.com/) also documents an OpenCypher-based alternative. It was reviewed but is outside the first implementation comparison.

We have not audited every database's security, performance, or licensing terms. Record the exact selected server and driver versions on day one; use stable compatible versions and keep those versions fixed during the sprint.

## Why Neo4j is the initial choice

The official driver gives C# a direct supported integration path. We can store dependency properties, traverse a bounded number of hops, and inspect results in the database's tooling. This reduces the number of unfamiliar integrations we must debug at once.

The choice is provisional. A small proof of concept should verify connectivity, constraints, import transactions, the representative impact query, restart persistence, and acceptable resource usage on the actual development machine.

No second database is required for version 0.1. Adding another database means extra configuration, backup, consistency, and testing work.

## Time comparison for later pilots

These are planning estimates for a solo developer learning while building, not measured project results:

| Candidate | Estimated pilot effort | Approximate weeks at 15 hours/week |
| --- | ---: | ---: |
| Dependency-impact explorer | 230–350 hours with uncertainty allowance | 16–24 |
| Document-request tracker | 220–340 hours | 15–23 |
| Webhook-recovery tool | 250–400 hours | 17–27 |

The one-week RippleMap prototype removes login, integrations, alerts, history comparisons, multi-company hosting, and a polished interactive graph. It is a different scope from these pilot estimates.


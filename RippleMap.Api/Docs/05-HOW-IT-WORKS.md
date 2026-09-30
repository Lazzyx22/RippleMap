# RippleMap: how everything fits together

This is the beginner explanation for the prototype. Read it alongside the diagrams and contracts, then return to a concept when you implement it.

## 1. The problem becomes data

A statement such as “the web application uses checkout, and checkout uses the payment provider” contains three resources and two relationships.

In a graph, a resource is a node. A relationship is a directed edge. A node or edge can also have properties: name, owner, verification date, or whether a dependency is required.

The data model matters because it determines which questions are easy to ask. If we stored only a screenshot of a diagram, searching indirect dependencies would be difficult. Explicit nodes and edges let a program follow connections.

## 2. Why direction changes the answer

If checkout depends on orders-db, an orders-db change may affect checkout. A checkout change does not automatically affect the database.

To find what a resource uses, follow outgoing edges. To find what relies on it, follow incoming paths. RippleMap's impact operation does the second.

A direct dependency is one edge away. A transitive dependency is reached through intermediate resources. The web application can be indirectly affected by the payment provider even when the web application never calls that provider itself.

## 3. What Cypher expresses

This teaching query matches resources that directly depend on a target:

```cypher
MATCH (dependent:Resource)-[:DEPENDS_ON]->(target:Resource)
WHERE dependent.datasetId = $datasetId
  AND target.datasetId = $datasetId
  AND target.id = $resourceId
RETURN dependent.id, dependent.name;
```

Read it in parts:

- MATCH describes a pattern to find.
- Parentheses identify nodes.
- Resource is the node label.
- The bracketed DEPENDS_ON is the relationship type.
- The arrow expresses direction.
- WHERE restricts the match.
- Dollar-prefixed names are parameters supplied separately by the application.
- RETURN chooses the result values.

A variable-length pattern follows several edges. The full bounded query in the contract also excludes repeated nodes and limits results.

The browser never writes this query. The application owns a fixed query and supplies validated parameter values. Parameterisation prevents a resource ID from being interpreted as query syntax.

## 4. How JSON becomes C# objects

JSON is a text format for exchanging structured data. The browser sends the map as JSON. ASP.NET Core deserialises it into request objects.

A simple illustrative DTO is:

```csharp
public sealed record ResourceInput(
    string Id,
    string Name,
    string Kind,
    string Owner);
```

The Id is stable identity. Name is display text. Kind is a validated category. Owner is a team label in this version.

The object is only input. A string property can still be missing, blank, too long, or inconsistent with another object. Deserialisation and validation solve different problems.

A validated map is the internal representation after all of those checks pass. Keeping that distinction clear prevents persistence code from becoming a mixture of parsing, error messages, and database commands.

## 5. What each application component does

| Component | Input | Work | Output |
| --- | --- | --- | --- |
| Controller | HTTP request | Bind input and map use-case result to HTTP | Status and JSON |
| Import validator | Request objects | Check fields and relationships | Errors or valid input |
| Normaliser | Valid map | Stable ordering and value formats | Canonical map |
| Import service | Canonical map | Decide create, unchanged, or conflict | Import result |
| Impact service | Dataset, target, depth | Enforce query rules and organise results | Impact response |
| Graph store | Valid operations | Run Cypher and transactions | Plain application data |
| Driver | Queries and parameters | Communicate with Neo4j | Database records |
| Browser | API responses | Display results and gather user choices | New HTTP requests |

A controller should not know how every graph query is written. The graph store should not decide HTTP status codes. Each part should have one understandable responsibility.

## 6. Dependency injection

A service needs collaborators. ImpactService needs something that can read the graph. Instead of constructing its own database driver, it receives a graph-store dependency.

At startup, Program.cs registers which implementation should be used. ASP.NET Core constructs the objects and supplies the required dependencies.

This helps tests and ownership. An application test can supply a fake store to verify result handling; a database integration test uses the real Neo4j store. A fake does not replace tests of Cypher correctness.

Keep one long-lived driver according to the driver's lifecycle guidance and use separate short-lived sessions for operations. Do not share one session across concurrent requests. The official driver documentation explains that sessions are not thread-safe.

Reference: [Neo4j transaction/session guidance](https://neo4j.com/docs/dotnet-manual/current/transactions/).

## 7. Async does not mean “create a new thread”

Database requests spend time waiting for I/O. An asynchronous method allows the application to wait without blocking a request thread unnecessarily.

await means resume this operation after the awaited work completes. It does not make slow queries fast and does not automatically run CPU-heavy work in parallel.

Pass cancellation where supported and configure database deadlines. Verify the driver's actual cancellation behaviour rather than assuming a CancellationToken on your interface stops every database operation.

## 8. Transactions prevent half a map

An import might create a dataset node, six resources, and five relationships. If writing the fifth relationship fails, keeping the earlier changes would leave misleading data.

A transaction groups the writes. Commit makes the complete operation visible. Rollback discards the operation if it fails.

The unique constraints enforce identities at the database level. Application validation alone cannot stop two simultaneous requests from both believing an ID is unused.

Neo4j managed transactions may retry transient failures. The callback must not send emails, write files, or perform other side effects that could happen twice. Map the result into ordinary C# values before leaving the session.

## 9. Repeated requests are normal

A user can double-click. A browser can lose a response. A connection can fail after the database commits.

If the second request blindly appends nodes, the graph gets duplicates. RippleMap instead compares dataset ID and canonical content hash.

- New ID: create once.
- Existing ID with identical contents: report unchanged.
- Existing ID with different contents: report conflict.

The content hash is a deterministic fingerprint, not encryption. It neither hides the map nor proves who authored it.

Immutable snapshots also make reading easier. A query does not need to combine parts of a dataset that are changing during the import. The whole snapshot appears after commit.

## 10. Validation protects meaning as well as code

The validator checks structural correctness: supported fields, valid types, legal IDs, and size limits. It also checks relationships: every from/to endpoint must exist, no duplicate directed pair, and no self-dependency.

A longer cycle can be real. Service A can call B while B also calls A. The map accepts that cycle; traversal limits and repeated-node checks keep the query controlled.

Validation cannot prove the declaration matches reality. source and lastVerifiedAt describe evidence supplied by the user. They do not independently verify it.

## 11. What a bounded answer means

Suppose the maximum depth is four. A resource five edges away is outside the answer even if it exists.

Suppose more than 100 paths match. Returning the first 100 does not prove every affected resource is represented. The API signals the cap, and the UI displays it.

The query timeout is another boundary. A configured three-second budget helps prevent expensive graph work from running indefinitely; it is not a hard real-time guarantee of a three-second HTTP response.

These limits are part of the result, not hidden implementation details. They help the user make sense of an incomplete map.

## 12. How the browser works

The initial page is served from the same application as the API. JavaScript uses fetch to send requests and read JSON responses. HTML and CSS display the data.

The interface needs loading, success, empty, and failure states. A 404 target, a 503 database failure, and a successful empty result should not look identical.

Imported names are untrusted display text. Use textContent or properly escaping templates. Do not concatenate them into executable HTML.

A visual graph is helpful for navigation, but an explanatory path table is often easier to understand. The first version can be useful with that table alone.

## 13. How export works

The application reads the dataset's resources and relationships and returns the documented import format.

An export is successful only if it can be read back meaningfully. The integration test exports, validates, and reimports the result in a fresh instance. Display order may change, but the graph's declared meaning should stay the same.

This is the week-one file-handling feature. Binary document uploads, attachments, malware scanning, and object storage are different product requirements and are not part of this graph tool's first release.

## 14. What tests prove

Unit tests examine rules such as field validation, content hashing, result grouping, or HTTP status mapping. They should run quickly and identify a specific broken rule.

Integration tests exercise the driver and actual Neo4j behaviour. They prove that the constraints, Cypher, and transaction assumptions work on the chosen version.

HTTP tests check that the application's public contract matches the implementation. A browser walkthrough checks that a human can perform the intended task.

No single kind of test proves everything. A fake store cannot prove a query finds the correct paths. A screenshot cannot prove an import rolls back.

## 15. Persistence, readiness, and logs

Neo4j data must live in persistent storage, such as a named volume in the chosen local container setup. Restarting the API should not remove graph data.

Liveness answers whether the API is running. Readiness answers whether its essential database dependency is usable. Treating them as identical makes failure diagnosis harder.

Logs should answer which operation failed and how to connect related events. They should avoid credentials and full imported maps. The request identifier is an operational handle, not a personal identifier.

## 16. What to explain at the end of the week

You should be able to trace these without relying on memorised definitions:

1. A JSON file entering the browser and becoming database records.
2. A repeated import returning unchanged.
3. A failed transaction preserving the previous state.
4. A graph query finding an indirect dependent.
5. A cycle terminating within limits.
6. A response becoming a readable path in the interface.
7. A configured limitation changing what the answer means.

If a step is unclear, use a smaller fixture and the debugger. Understanding one complete path through the system is more useful than adding another feature you cannot explain.


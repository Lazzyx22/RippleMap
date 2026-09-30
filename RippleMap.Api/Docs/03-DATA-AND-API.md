# RippleMap: data model and API contract

Version 0.1 design contract. The snippets are implementation specifications and teaching examples; they have not been compiled or executed against Neo4j while preparing these documents.

## Canonical input fixture

Save this content as samples/demo-map.json when you create the repository. The IDs describe fictional resources.

```json
{
  "schemaVersion": 1,
  "datasetId": "demo-v1",
  "resources": [
    { "id": "web-app", "name": "Web application", "kind": "application", "owner": "web-team" },
    { "id": "mobile-app", "name": "Mobile application", "kind": "application", "owner": "mobile-team" },
    { "id": "checkout", "name": "Checkout service", "kind": "service", "owner": "commerce-team" },
    { "id": "payment-provider", "name": "Payment provider", "kind": "external-api", "owner": "commerce-team" },
    { "id": "orders-db", "name": "Orders database", "kind": "database", "owner": "platform-team" },
    { "id": "refunds", "name": "Refund service", "kind": "service", "owner": "commerce-team" }
  ],
  "dependencies": [
    { "from": "web-app", "to": "checkout", "required": true, "source": "manual:demo", "lastVerifiedAt": "2026-09-30T00:00:00Z" },
    { "from": "mobile-app", "to": "checkout", "required": true, "source": "manual:demo", "lastVerifiedAt": "2026-09-30T00:00:00Z" },
    { "from": "checkout", "to": "payment-provider", "required": true, "source": "manual:demo", "lastVerifiedAt": "2026-09-30T00:00:00Z" },
    { "from": "checkout", "to": "orders-db", "required": true, "source": "manual:demo", "lastVerifiedAt": "2026-09-30T00:00:00Z" },
    { "from": "refunds", "to": "payment-provider", "required": true, "source": "manual:demo", "lastVerifiedAt": "2026-09-30T00:00:00Z" }
  ]
}
```

The input supplies the map, not database commands. The server never interprets source as a URL to fetch, and never accepts arbitrary Cypher from this payload.

## Validation rules

| Field or condition | Version 0.1 rule |
| --- | --- |
| schemaVersion | Exactly 1 |
| datasetId and resource IDs | Match ^[a-z][a-z0-9-]{0,63}$ |
| resources | Between 1 and 100 |
| dependencies | Between 0 and 300 |
| Resource IDs | Unique within the payload |
| name | Required; trimmed length 1–100 |
| kind | application, service, database, or external-api |
| owner | Required; trimmed length 1–80; may be unassigned |
| from and to | Must refer to resource IDs in this same payload |
| Duplicate edge | Same from/to pair is rejected |
| Self-dependency | from equal to to is rejected |
| Longer cycles | Accepted; queries must handle them safely |
| required | An explicit boolean |
| source | Required; trimmed length 1–200 |
| lastVerifiedAt | Required ISO-8601 timestamp with an offset; normalise to UTC |
| Request body | Maximum 1 MiB, enforced by the server |
| Unknown JSON fields | Reject to catch misspellings and unsupported versions |
| Empty strings or null required fields | Reject |
| Query depth | Integer from 1 through 6; default 4 |

These are product limits chosen for a one-week prototype, not limits of Neo4j.

Return all practical field-validation errors from one request so the user can correct the file once. Do not run database writes while still validating the payload.

## Graph representation

Every imported resource has the Resource label. Use its kind property for the four resource categories in week one. This avoids dynamically constructing labels from input.

| Graph element | Stored values |
| --- | --- |
| Dataset node | id, schemaVersion, contentHash, createdAt, resourceCount, relationshipCount |
| Resource node | datasetId, id, name, kind, owner |
| DEPENDS_ON relationship | required, source, lastVerifiedAt |

A relationship points from the dependent to its dependency. All resources and edges belonging to one dataset are inserted together.

Create uniqueness constraints during database setup:

```cypher
CREATE CONSTRAINT dataset_identity IF NOT EXISTS
FOR (d:Dataset) REQUIRE d.id IS UNIQUE;
```

```cypher
CREATE CONSTRAINT resource_identity IF NOT EXISTS
FOR (r:Resource) REQUIRE (r.datasetId, r.id) IS UNIQUE;
```

Verify these commands against the selected Neo4j Community version during setup. They are property-uniqueness constraints, not Enterprise-only node-key constraints.

The Resource identity is composite because checkout in demo-v1 and checkout in demo-v2 are different snapshot records.

## Import ownership and content hashes

Datasets are immutable. A first import returns 201 created. The same ID with the same normalised content returns 200 unchanged. The same ID with different normalised content returns 409 conflict.

Canonicalisation must be deterministic:

1. Validate IDs rather than silently changing their case.
2. Trim the specified display fields.
3. Normalise timestamps to a consistent UTC representation.
4. Sort resources by ID.
5. Sort dependencies by from, then to.
6. Serialise the normalised DTO using a fixed property order and serializer configuration.
7. Hash its UTF-8 bytes using SHA-256.

Include schemaVersion, datasetId, resources, and dependencies in the hash. Exclude server metadata such as createdAt and the hash itself. JSON whitespace and array ordering in the input should not cause a different hash after normalisation.

Test the canonicaliser. A hash is useful only if the data fed into it is stable. This design does not claim cross-language canonical JSON compatibility.

Create the Dataset node and graph contents in one managed write transaction. If any query or integrity check fails, throw and roll back. Consume each query result before proceeding. Transaction callbacks may retry, so do not send notifications, write external files, or generate different business IDs inside them.

Neo4j documents the transaction and parameter behaviour in its [official .NET transaction guide](https://neo4j.com/docs/dotnet-manual/current/transactions/).

## Planned HTTP endpoints

| Method and path | Purpose | Success |
| --- | --- | --- |
| GET /health/live | Application responds | 200 |
| GET /health/ready | Application can reach its configured graph database | 200; 503 if unavailable |
| POST /api/imports | Validate and commit a complete dataset | 201 created or 200 unchanged |
| GET /api/datasets | List imported dataset metadata | 200 |
| GET /api/datasets/{datasetId}/resources?query=checkout | Search up to 100 resources by ID or display name | 200 |
| GET /api/datasets/{datasetId}/impact/{resourceId}?maxDepth=4 | Explain incoming dependency paths | 200 |
| GET /api/datasets/{datasetId}/export | Export the map in the accepted input format | 200 JSON download |

The prototype stores no uploaded file on a server filesystem. It parses JSON into graph records. The browser sends JSON in the request body; multipart uploads are unnecessary.

For exports, reconstruct the documented payload. Server-only metadata does not belong in the export. Use a server-generated download filename derived from the validated dataset ID.

## Error contract

Use an HTTP Problem Details-shaped response with a stable machine-readable code and optional validation errors. Do not expose database credentials, connection strings, stack traces, or raw Cypher errors.

Illustrative validation response:

```json
{
  "type": "about:blank",
  "title": "Dependency map is invalid",
  "status": 400,
  "code": "invalid_map",
  "errors": {
    "dependencies[0].to": ["Resource 'missing-service' does not exist in resources."]
  }
}
```

| Status | Example code | Meaning |
| --- | --- | --- |
| 400 | invalid_map / invalid_depth | Invalid input |
| 404 | dataset_not_found / resource_not_found | Target does not exist |
| 409 | dataset_conflict | Existing dataset ID has different contents |
| 413 | payload_too_large | Body exceeds the configured limit |
| 503 | graph_unavailable | Database cannot currently serve the request |
| 504 | query_timed_out | Application's graph-query deadline was exceeded |

Infrastructure exceptions must be mapped deliberately. Do not classify every unexpected exception as a user error.

## Impact-query semantics

The target is excluded from the affected-resource list. Consider only paths fully inside the selected dataset. A path must not repeat a node. All declared dependencies are included, including optional ones; the response exposes required so the UI can explain the distinction.

Depth is the number of edges, not the number of nodes. A path with three resources has depth two.

Use a fixed allowlist of query variants for depths 1–6. Select the appropriate variant after parsing and validating the integer. Parameterise dataset IDs, resource IDs, and result limits. Do not interpolate arbitrary request text into Cypher.

Example of the depth-4 variant:

```cypher
MATCH p = (r:Resource {datasetId: $datasetId})
  -[:DEPENDS_ON*1..4]->
  (target:Resource {datasetId: $datasetId, id: $resourceId})
WHERE r.id <> target.id
  AND all(n IN nodes(p) WHERE n.datasetId = $datasetId)
  AND all(n IN nodes(p)
          WHERE single(m IN nodes(p) WHERE m = n))
RETURN
  [n IN nodes(p) | n.id] AS resourceIds,
  [e IN relationships(p) |
    {required: e.required,
     source: e.source,
     lastVerifiedAt: toString(e.lastVerifiedAt)}] AS dependencies
LIMIT $fetchLimit;
```

For this version, fetchLimit is 101 and the response returns at most 100 paths. Run the query under a configured 3-second transaction/query deadline, then verify timeout behaviour with the selected driver and server.

LIMIT caps returned rows; it does not by itself guarantee cheap query execution. Bounded dataset size, maximum depth, exclusion of repeated nodes, and a timeout all matter. Dense graphs can still be expensive.

Do not label a returned path as the shortest unless a separate shortest-path algorithm actually establishes that. Version 0.1 returns explanatory paths, and their ordering is unspecified.

If 101 paths are returned, discard the extra path and set resultLimitReached to true. An affected-resource list derived from those paths can then be incomplete. Even when false, the result only covers the chosen depth and recorded data.

## Expected example result

For demo-v1, target payment-provider, and maxDepth 4:

| Starting resource | Explanatory path |
| --- | --- |
| checkout | checkout → payment-provider |
| refunds | refunds → payment-provider |
| web-app | web-app → checkout → payment-provider |
| mobile-app | mobile-app → checkout → payment-provider |

Expected counts: 4 paths and 4 affected resources. orders-db is not affected by this target according to the recorded dependency directions.

The HTTP response should contain datasetId, targetId, maxDepth, returnedPathCount, returnedResourceCount, resultLimitReached, paths, and affectedResources. Each path contains resourceIds and relationship metadata. These are expected fixture results, not measurements from a running implementation.

Changing maxDepth to 1 should return only checkout and refunds. Changing the target to orders-db at depth 2 should return checkout, web-app, and mobile-app.

## Searching and exporting

Search can use a case-insensitive contains match against ID and name, since a dataset has at most 100 resources. Limit query text to 80 characters. Avoid adding a full-text indexing subsystem before there is a demonstrated need.

Exported JSON should pass the same validator. Reimport into the same installation should return unchanged. In a fresh installation, it should create the same resource/edge content. These tests verify that export is usable data, not just a download button.

## Future schema changes

Version 1 is deliberately small. A future schema may add multiple edges between the same resources, typed relations, optionality semantics, versions, or separate team nodes. Such changes need a schema version and a compatibility plan. Never reinterpret old data silently.

The database model is an implementation detail; the public JSON and HTTP contracts should not expose Neo4j's internal node IDs.


# RippleMap: quality standards and later roadmap

Version 0.1 is a local prototype. The later pilot is a separate scope. The estimates here are planning ranges for one developer learning while implementing and should be revised after the first week.

## Quality goals for week one

| Area | Required behaviour |
| --- | --- |
| Correctness | Dependency direction and indirect paths match the fixture |
| Input handling | Invalid maps fail before persistence with useful errors |
| Integrity | Complete import commits or rolls back |
| Repeated requests | Identical import does not duplicate data |
| Isolation | A dataset query never returns another dataset's resources |
| Resource limits | Body size, graph size, depth, result cap, and query budget are enforced |
| Output safety | Imported display text cannot become executable HTML |
| Maintainability | HTTP mapping, use cases, validation, and Neo4j code have clear boundaries |
| Reproducibility | Selected versions, setup, configuration, and tests are documented |

“Enterprise-grade” remains a quality ambition. No seven-day prototype should claim that label without evidence of its operational behaviour and limitations.

## Meaningful acceptance tests

| Test | Setup | Expected result |
| --- | --- | --- |
| Valid import | Canonical demo fixture | 201; six resources and five relationships |
| Repeated import | Send the same fixture again | 200 unchanged; stable counts |
| Equivalent ordering | Reverse resource/edge array order | Still unchanged after normalisation |
| Changed data, same ID | Change a resource name | 409; existing map preserved |
| Invalid endpoint | Add edge to an absent ID | 400; no new dataset |
| Duplicate node ID | Repeat checkout | 400 with field-level error |
| Self-dependency | checkout depends on itself | 400 |
| Longer cycle | A depends on B, B depends on A | Accepted map; bounded query terminates |
| Direct impact | payment-provider, depth 1 | checkout and refunds |
| Indirect impact | payment-provider, depth 2 | checkout, refunds, web-app, mobile-app |
| Direction correctness | payment-provider selected | orders-db excluded |
| Missing target | Unknown ID | 404, not successful empty output |
| No dependents | Existing isolated node | 200 with empty paths |
| Result cap | More than 100 matching paths | At most 100 returned; cap flag true |
| Dataset separation | Same IDs in two datasets | Results come only from selected dataset |
| Mid-import error | Fail within write transaction | No partial dataset or orphan resources |
| Concurrent import | Two requests with same dataset ID | One consistent stored map; correct second response |
| Database unavailable | Stop isolated test database | 503; no false success |
| Query timeout | Controlled expensive test case | Clear query_timed_out failure |
| Persistence | Restart API and database safely | Committed data remains |
| Safe rendering | Name includes HTML-like content | Browser shows literal text |
| Export round trip | Export and validate/reimport | Equivalent graph content |

Use an isolated database and synthetic data for destructive or failure tests. Do not run reset scripts against personal or company databases.

The documentation package checks do not constitute these application tests. They become release evidence only after the actual application has been implemented and run.

## Clean-code rules with a reason

- Keep controllers short enough that the HTTP behaviour is easy to see.
- Give services use-case names rather than vague names such as Manager.
- Keep validation separate from writes so failure cannot leave partial data.
- Return application results instead of database-driver records from service boundaries.
- Use parameterised values in Cypher.
- Prefer explicit data mappings while learning.
- Use nullable-reference checking to expose assumptions.
- Preserve useful exception context in logs while returning safe client errors.
- Avoid catch blocks that return success or empty data after a failure.
- Keep the dependency list small and explain each package.
- Add an abstraction only when it clarifies a boundary or removes demonstrated duplication.

These are review criteria, not a requirement to create many projects or patterns.

## System-design decisions to record

Keep one short decision record for each significant choice:

| Decision | Selected approach | Trade-off to explain |
| --- | --- | --- |
| Product | Focused dependency-impact explorer | Smaller workflow; existing competitors remain |
| Database | Neo4j Community | Graph learning and traversal fit; additional runtime to operate |
| Deployment shape | One API and one database | Easier local operation; not independently scalable modules |
| Imports | Immutable whole datasets | Predictable snapshots; no in-place editing |
| Idempotency | Dataset ID plus canonical content hash | Repeat-safe imports; canonicalisation must be correct |
| Impact | Bounded declared paths | Explainable results; incomplete beyond configured bounds |
| UI | Simple list and path table | Useful quickly; less visual polish |
| Security scope | Local single-user prototype | Faster learning scope; unsuitable for shared company access |
| Ownership | Owner string initially | Simple; richer team relationships deferred |

Each record should include the problem, alternatives, choice, consequences, and what evidence would make us revisit it.

## Mapping the original nine goals to this product

| Goal | Week-one evidence | Later extension |
| --- | --- | --- |
| Why it is needed | Research and a reproducible dependency-navigation task | Trials with small teams and comparison against existing tools |
| System design | Defined boundaries, contracts, graph direction, and failure flows | Measured scale and operational decisions |
| Clean code | Focused modules, naming, validation, parameterised queries | Refactoring based on actual change patterns |
| Enterprise-grade quality | A limited, tested correctness baseline | Access control, restore testing, dependency review, observability, and operational ownership |
| Faster deployment | Reproducible local setup with pinned dependencies | CI-built image, controlled release, smoke checks, and rollback planning |
| Customizable templates | Fixed documented import schema | Import mappings and report templates after a second real format is needed |
| Seamless file handling | JSON validation, useful errors, safe export | Additional formats, upload limits, and retention policies where justified |
| Reduced maintenance overhead | Few dependencies, stable contracts, explicit ownership | Versioned schemas, migration tooling, and automated regression checks |
| Enhanced service layer | ImportService and ImpactService separated from HTTP and graph I/O | Additional use cases with similarly explicit boundaries |

The original API generator is a possible later tool, not the project's foundation. Generate repetitive code only after repeated patterns emerge in working features.

## Pilot effort retained from the broader plan

| Module | Estimated focused hours |
| --- | ---: |
| Problem validation | 8–12 |
| Graph fundamentals and database trial | 16–24 |
| Resource catalog and API | 20–30 |
| Import pipeline | 20–30 |
| Impact analysis | 24–36 |
| Web interface | 30–45 |
| Access control | 18–28 |
| History and freshness | 18–28 |
| Release preparation and operational tests | 30–45 |
| **Base total** | **184–278** |
| **With approximately 25% uncertainty** | **230–350** |

This is total effort from the start of the broader project, not an extra 230–350 hours on top of prototype work. Some prototype work contributes directly; some will need refactoring.

At 15 hours per week, the broader pilot remains approximately 16–24 weeks. Finishing a useful local subset in a week does not imply that every pilot feature has also been completed.

## Proposed releases

### Version 0.1: local feasibility

Complete the seven-day acceptance scope: import, inspect, explain, export, and reproduce. The result answers the core question using a small explicit map.

### Version 0.2: team pilot

Add login, viewer/editor permissions, and access-control tests. Introduce controlled update/version workflows and display relationship freshness. Add backup/restore instructions and exercise them.

Before using real company data, review who can access maps and exports. Dependency information can reveal internal system structure even without customer records.

### Version 0.3: connected data sources

Implement one integration chosen by an actual user. Start with a repository-held map or one dependency-manifest format. Track the source and observation time.

Handle deleted resources, renamed identifiers, sync failures, rate limits, and stale observations. Do not merge package and runtime dependency semantics as though they were the same.

### Version 0.4: workflow extensions

Add configurable import mappings, export/report templates, and selected change notifications if users need them. A template should customise presentation or mapping rules without executing arbitrary untrusted code in the server.

Multi-company hosting, billing, organisation isolation, distributed workers, and high availability require a separate plan. They are not hidden tasks inside these releases.

## Optional additions and estimates

| Extension | Additional effort beyond the core scope |
| --- | ---: |
| One repository integration with scheduled refresh | 25–45 hours |
| One dependency manifest or SBOM importer | 25–45 hours |
| Configurable import mappings and report templates | 20–35 hours |
| Dependency-change notifications | 15–25 hours |
| Graph database comparison using representative fixtures | 12–24 hours |

These are bounded experiments. Integration complexity can grow if a provider's API, authentication, or data semantics are unfamiliar.

## Open-source readiness

A usable first repository should contain:

- A README showing the actual task the tool solves.
- Exact local setup instructions.
- Synthetic sample data and a reproducible demonstration.
- Tests and a simple CI workflow when ready.
- Known limits and unsupported features.
- A consciously selected license.
- Contribution instructions and issue templates.
- A way to report sensitive security problems privately.
- Decision records that explain important trade-offs.

Do not invent users, performance claims, time savings, or production deployments. Measure before claiming a benefit.

## How to judge whether to continue

After the prototype, ask someone to perform a real, sanitised dependency task. Observe where they get confused and whether the result changes a useful decision.

Continue if the workflow helps, the data can be kept current, and the maintenance burden is acceptable. Simplify or contribute upstream if an existing tool meets the need better.

A successful week can produce a working experiment and a better understanding of the problem. It does not require pretending the product is finished.


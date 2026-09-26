# Phased Implementation Plan

Status: Complete and verified  
Last updated: 2026-08-15

Each phase is independently buildable and has an explicit exit criterion. Small red/green/refactor loops are used inside phases.

## 1. Solution and engineering baseline

- **Goal:** Reproducible .NET 10 repository.
- **Implementation:** Create solution/projects, pin SDK, centralize package versions and compiler/analyzer settings, add local Stryker manifest and editor/format rules.
- **Deliverables:** Six projects, dependency references, `Directory.Build.props`, `Directory.Packages.props`, `global.json`, tool manifest.
- **Tests:** Template smoke tests removed/replaced.
- **Validation:** Restore and build the empty architecture.
- **Exit criteria:** Correct dependency direction, nullable enabled, warnings are errors, no restore/build warnings.

## 2. Domain model and validation

- **Goal:** Immutable, bounded scheduling inputs/outputs.
- **Implementation:** Job/options/identifier/result records, validation errors, canonical ordering.
- **Deliverables:** Domain contracts with unambiguous time names and ranges.
- **Tests:** Valid/invalid boundaries, uniqueness, empty input, overflow/resource bounds.
- **Validation:** Domain test project passes independently.
- **Exit criteria:** Invalid states cannot enter an algorithm unnoticed.

## 3. Timeline recorder and metrics

- **Goal:** One trustworthy result pipeline before algorithms proliferate.
- **Implementation:** State snapshots, event sequencing, execution/idle slices, result factory, invariant validator, metrics calculator.
- **Deliverables:** Deterministic event stream and metric formulas.
- **Tests:** Independently calculated metric cases and deliberately invalid timelines.
- **Validation:** Slice accounting and structural equality tests.
- **Exit criteria:** Recorder can represent arrival, queue, dispatch, run, preempt, resume, complete, idle, and boost.

## 4. FCFS vertical slice

- **Goal:** Prove the engine/result design with the simplest scheduler.
- **Implementation:** FCFS strategy through shared recorder/result factory.
- **Deliverables:** Complete FCFS result with explanations.
- **Tests:** Golden ordering, simultaneous arrivals, initial/intermediate idle, single and empty workloads.
- **Validation:** Manual timeline/metric comparison.
- **Exit criteria:** FCFS golden and invariant tests pass without UI dependencies.

## 5. SJF and SRTF

- **Goal:** Add shortest-work non/preemptive contrast.
- **Implementation:** SJF dispatch scan and SRTF remaining-time reevaluation.
- **Deliverables:** Two strategies and metadata.
- **Tests:** Shortest available, non-preemption, equal ties, strict shorter preemption, equal remaining behavior, multiple preemptions.
- **Validation:** Golden slices and metrics.
- **Exit criteria:** Both strategies pass shared invariants and deterministic-repeat tests.

## 6. Round Robin

- **Goal:** Make time slicing and queue rotation explicit.
- **Implementation:** FIFO queue, quantum accounting, boundary-arrival ordering.
- **Deliverables:** RR strategy and quantum option.
- **Tests:** Expiry, partial/completing quantum, arrivals during execution, repeated rounds, solo renewal, queue order.
- **Validation:** Hand-traced queue snapshots.
- **Exit criteria:** Every requeue/preemption is explained in events.

## 7. Priority schedulers

- **Goal:** Demonstrate static priority and preemption trade-offs.
- **Implementation:** Non-preemptive and strict-higher preemptive strategies.
- **Deliverables:** Two strategies.
- **Tests:** priority order, equal ties, arrival behavior, repeated strict preemption.
- **Validation:** Starvation trade-off copy matches semantics.
- **Exit criteria:** Equal priority never causes an accidental preemption.

## 8. HRRN, MLQ, and MLFQ

- **Goal:** Complete first-release scheduler set.
- **Implementation:** Cross-multiplied HRRN ratios; fixed three-level MLQ; dynamic three-level MLFQ with boost.
- **Deliverables:** Three strategies, queue/boost options and events.
- **Tests:** HRRN aging/ties; MLQ inter/intra-queue rules; MLFQ entry, demotion, higher-level preemption, retained quantum, boost ordering, starvation prevention.
- **Validation:** Golden traces and shared invariants.
- **Exit criteria:** Nine algorithms produce deterministic valid results.

## 9. Application orchestration and sample jobs

- **Goal:** Stable use cases independent of Razor.
- **Implementation:** Catalog, multi-run request/result, deterministic five-job generator, reset baseline, playback state machine.
- **Deliverables:** Application services and DI registration.
- **Tests:** same immutable dataset for comparisons, generator determinism/shape, all playback commands/speeds.
- **Validation:** Application test suite with substitutes only at boundaries.
- **Exit criteria:** Web can be a pure adapter over application state.

## 10. UI shell and job editor

- **Goal:** Build the accessible lab layout and valid workload editing.
- **Implementation:** Responsive shell, job table, seed/regenerate/reset, inline validation, queue and priority inputs.
- **Deliverables:** Functional workload console following the design tokens.
- **Tests:** rendering, edit/regenerate/reset, validation messages, keyboard labels.
- **Validation:** bUnit plus desktop/mobile visual inspection.
- **Exit criteria:** Five jobs are generated and safely editable without scheduling logic in Razor.

## 11. Algorithm selection and run workflow

- **Goal:** Select one/many algorithms against identical jobs.
- **Implementation:** metadata-driven cards/checks, contextual options, and a trace-building action located after policy selection. The action is disabled for zero policies; success collapses setup, records an experiment summary, transfers focus to results, and preserves an explicit edit route.
- **Deliverables:** algorithm selector and request validation.
- **Tests:** selection, option binding, action placement, disabled/error states, setup collapse/edit restoration, focus handoff, and same-dataset comparison.
- **Validation:** bUnit interactions plus real-browser flow inspection.
- **Exit criteria:** All algorithms can be run singly or together.

## 12. Scheduler debugger visualization

- **Goal:** Explain transitions, not merely show totals.
- **Implementation:** ready queue, dispatch gate, CPU chamber, completed area, event explanation, preemption loop driven solely by snapshots.
- **Deliverables:** event-by-event visual state with labels/icons.
- **Tests:** arrived/waiting/selected/running/preempted/completed/idle states.
- **Validation:** Golden UI replay walkthrough.
- **Exit criteria:** A learner can state why the current job runs and what happens next.

## 13. Timeline and playback

- **Goal:** Provide graphical history and deterministic single/shared animation control.
- **Implementation:** focused SVG/CSS Gantt rail; synchronized multi-lane SVG using one absolute scale; single and comparison current-time cursors; start/pause/resume/step/restart/reset; 0.5×/1×/2×/4× timer mapping.
- **Deliverables:** complete single-policy timeline, common comparison axis, and replay controls.
- **Tests:** single/shared state-machine transitions, union decision stepping, projected result state, component control transitions, and SVG lane/segment data.
- **Validation:** speed changes leave results structurally identical; reduced-motion inspection.
- **Exit criteria:** step mode communicates the entire schedule with motion disabled.

## 14. Comparison and teaching notes

- **Goal:** Compare execution and outcomes without declaring a universal winner.
- **Implementation:** one shared clock; aligned algorithm swimlanes; responsive 2-column compact ready/CPU/done debuggers; optional focused full-event inspector; readable metrics table and algorithm-specific trade-off notes.
- **Deliverables:** simultaneous execution state plus average wait/turnaround/response, utilisation, throughput, context switches, and makespan.
- **Tests:** one lane and compact card per selected result, synchronized stepping, no tab-only comparison path, focused-inspector opening, and metrics binding.
- **Validation:** Cross-check the table against golden expected metrics and inspect 1440 px/390 px layouts in a real browser.
- **Exit criteria:** Comparison uses exactly the same jobs, displays every selected algorithm together at the same tick, and explains trade-offs.

## 15. Test hardening

- **Goal:** Attempt to break all algorithms.
- **Implementation:** required edge cases, shared invariants, seeded randomized workloads, structural determinism, component state coverage.
- **Deliverables:** behavioral/golden/invariant/UI suites.
- **Tests:** This phase is the suite expansion.
- **Validation:** repeated full test runs and seed replay.
- **Exit criteria:** No required scenario is missing; no test derives expected golden output from production.

## 16. Coverage review

- **Goal:** Find meaningful untested behavior.
- **Implementation:** collect Cobertura, summarize line/branch/method coverage, inspect uncovered sources and classify gaps.
- **Deliverables:** coverage artifacts and report section.
- **Tests:** Add tests for important gaps only.
- **Validation:** rerun coverage after improvements.
- **Exit criteria:** Domain scheduling has very high meaningful coverage and every significant gap is explained.

## 17. Mutation testing

- **Goal:** Measure whether tests detect changed scheduler behavior.
- **Implementation:** run Stryker against algorithms/timeline/metrics, inspect survivors, strengthen assertions.
- **Deliverables:** HTML/JSON/Markdown mutation reports.
- **Tests:** Targeted tests for meaningful survivors.
- **Validation:** rerun mutation scope.
- **Exit criteria:** Score and justified survivors are reported from actual output.

## 18. Security and dependency review

- **Goal:** Minimize client attack surface and supply-chain risk.
- **Implementation:** direct/transitive vulnerable, deprecated, and outdated package checks; input/rendering/overflow/workload review.
- **Deliverables:** scan output and security findings.
- **Tests:** malicious-looking IDs, numeric extremes, invalid configs, workload cap.
- **Validation:** no raw HTML/secrets/network/persistence and no known vulnerable production dependencies.
- **Exit criteria:** Critical/Important security findings fixed.

## 19. Static analysis and formatting

- **Goal:** Warning-free, consistently formatted source.
- **Implementation:** build analyzers and `dotnet format`; inspect suppressions.
- **Deliverables:** clean source and verification output.
- **Tests:** Full suite after formatting.
- **Validation:** zero errors/unexpected warnings and format verification passes.
- **Exit criteria:** No blanket warning suppressions or unexplained analyzer exclusions.

## 20. Architecture/UX review and refactor

- **Goal:** Review as an independent senior PR reviewer.
- **Implementation:** inspect SOLID, coupling, duplication, UI/domain leakage, class sizes/names/state, extension path, responsive/a11y/reduced motion.
- **Deliverables:** classified review notes and fixes.
- **Tests:** Regression suite.
- **Validation:** Perform the Lottery extension thought experiment and dependency inspection.
- **Exit criteria:** All Critical/Important findings fixed; remaining trade-offs documented.

## 21. README and clean final verification

- **Goal:** Reproducible evidence and handoff.
- **Implementation:** professional README with terminology, architecture, algorithms, screenshots/visual, commands, extension guide, limitations; clean verification from root.
- **Deliverables:** README and final engineering report.
- **Tests:** Restore, build, full tests, coverage, mutation, vulnerability/deprecation scan, format/static analysis, production build, startup/HTTP smoke test.
- **Validation:** Record actual counts/scores/output; never infer a pass.
- **Exit criteria:** Application starts and every applicable Definition of Done item is evidenced or explicitly listed as a known limitation.

## Current phase outcome

All phases and the synchronized comparison uplift are implemented. Final evidence: warning-free build and format verification; 61 passing tests; Domain coverage of 90.71% lines / 82.17% branches; 69.23% mutation score over the scoped engine; no known vulnerable or deprecated packages; responsive real-browser checks at 1440 px and 390 px; and successful local HTTP startup. Exact commands, counts, caveats, and extension guidance are maintained in the README.

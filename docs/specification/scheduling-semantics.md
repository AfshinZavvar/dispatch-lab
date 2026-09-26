# Scheduling Semantics

Status: Implemented and verified  
Scope: First release

## Workload model

The simulator models one logical CPU and independent CPU-bound jobs, each with one uninterrupted CPU burst except for scheduler preemption. There is no I/O blocking, process creation, multicore execution, context-switch cost, or deadline. Time is a non-negative integer measured in abstract **ticks**. Execution intervals are half-open: `[start, end)`.

Each job has:

- `Id`: unique, non-empty, display-safe text; generated values are `P1` through `P5`.
- `ArrivalTime`: first tick at which the job is eligible.
- `BurstTime`: exact CPU ticks required to complete.
- `Priority`: static priority from 1 (highest) through 99 (lowest).
- `QueueLevel`: fixed multilevel-queue class from 0 (interactive/highest) through 2 (background/lowest). It is separate from `Priority` because fixed queue membership and priority scheduling express different concepts.

At a boundary at time `t`, all jobs with `ArrivalTime == t` become ready before the scheduler selects work for `[t, ...)`. A job never runs before it arrives. If no job is ready, the CPU advances directly to the next arrival and records an idle slice.

## Determinism and global ties

All input is validated and copied to an immutable, canonical order. Whenever an algorithm-specific criterion is equal, ties are broken by:

1. earlier arrival time;
2. ordinal job ID (`StringComparer.Ordinal`).

Queue-based algorithms additionally preserve their explicit enqueue order. Jobs arriving at the same tick enter in the global tie order. No choice depends on input collection type, dictionary enumeration, UI position, unstable sorting, or randomness.

For preemptive algorithms, equality alone does not preempt the current job. This avoids meaningless switches while remaining deterministic.

## Algorithms selected for release 1

| Algorithm | Mode | Inputs | Selection and queue rule | Preemption | Release 1 |
|---|---|---|---|---|---|
| First Come First Served (FCFS) | Non-preemptive | Jobs | Oldest ready arrival, then ID; FIFO | Never | Yes |
| Shortest Job First (SJF) | Non-preemptive | Jobs | Smallest original burst among ready jobs, then global ties | Never | Yes |
| Shortest Remaining Time First (SRTF) | Preemptive | Jobs | Smallest remaining time; current job wins an equal-remaining tie | On arrival of a strictly shorter remaining job | Yes |
| Round Robin (RR) | Preemptive/time sliced | Quantum | FIFO ready queue | At quantum expiry when another job is ready; unfinished job moves to tail | Yes |
| Priority, non-preemptive | Non-preemptive | Static priority | Lowest numeric priority among ready jobs, then global ties | Never | Yes |
| Priority, preemptive | Preemptive | Static priority | Lowest numeric priority; current job wins equal priority | On arrival of a strictly higher-priority job | Yes |
| Highest Response Ratio Next (HRRN) | Non-preemptive | Jobs | Highest `(waiting + burst) / burst` among ready jobs | Never | Yes |
| Multilevel Queue (MLQ) | Strict fixed queues | Queue level, Q0/Q1 quanta | Highest non-empty queue: Q0 RR, Q1 RR, Q2 FCFS | A Q0/Q1 quantum expires; arrival in a higher queue preempts lower work | Yes |
| Multilevel Feedback Queue (MLFQ) | Preemptive adaptive queues | Per-level quanta, boost interval | Highest non-empty queue; RR within a level; new jobs enter Q0 | Quantum/allotment expiry demotes; higher-level arrival preempts; periodic boost prevents starvation | Yes |

### Algorithms evaluated but not included

- **Lottery scheduling:** valuable for teaching proportional-share scheduling but requires ticket allocation and seeded pseudo-random selection. It is retained as the architecture extensibility proof rather than adding another first-release input model.
- **Stride scheduling:** deterministic proportional share, but less commonly expected in an introductory CPU scheduling comparator and depends on the same share/ticket model as lottery scheduling.
- **Earliest Deadline First:** appropriate for real-time scheduling, but deadlines and missed-deadline semantics are outside the single-burst teaching model.
- **Linux Completely Fair Scheduler:** production-relevant but its virtual-runtime and weight model would add platform-specific complexity without improving the introductory comparison.

These exclusions follow YAGNI rather than algorithm-count maximization.

## Exact algorithm specifications

### FCFS

- Enqueue arrivals in global tie order.
- Dispatch the ready-queue head and run it to completion.
- Arrivals during execution join the tail and never preempt.
- Complexity: canonical sorting `O(n log n)`; queue operations `O(1)`.

### SJF

- At each dispatch, select the ready job with the least original `BurstTime`, then global ties.
- Run it to completion. Later shorter arrivals wait until the next dispatch.
- Complexity: `O(n²)` with a simple scan, acceptable for the bounded teaching workload.

### SRTF

- At dispatch and every arrival boundary, compare all ready jobs by remaining time, then global ties.
- Preempt only when a ready job has **strictly less** remaining time than the current job.
- A preempted job returns to the ready set with its exact remaining time.
- Complexity: `O(T × n)` in the intentionally tick-based reference engine, bounded by validation; this favors auditability over a heap implementation.

### Round Robin

- Positive `RoundRobinQuantum`; default 2 ticks.
- FIFO queue. A dispatch runs until completion, quantum expiry, or no earlier (arrivals do not interrupt a quantum).
- Arrivals during a quantum are enqueued at their arrival ticks. At an expiry boundary, boundary arrivals are enqueued before the unfinished running job returns to the tail.
- If the queue is otherwise empty, the same job continues with a new quantum; a decision boundary is recorded but it is not a context switch.
- Complexity: `O(T + n log n)`.

### Static priority

- Lower numeric `Priority` means higher scheduling priority.
- Non-preemptive mode selects by priority then global ties and runs to completion.
- Preemptive mode re-evaluates at arrivals and preempts only for a strictly lower numeric priority. Equal priority does not preempt.
- No aging is applied; the explanation warns that static priority may starve low-priority work under a continuing arrival stream.

### HRRN

- Non-preemptive. At each dispatch, for every ready job calculate `response ratio = (waiting time + burst time) / burst time`, where `waiting time = now - arrival` because the job has not yet run.
- Compare ratios with integer cross multiplication, not floating point. Equal ratios use global ties.
- This balances short-job preference with aging, but requires burst estimates.
- Complexity: `O(n²)`.

### Multilevel Queue

- Membership is fixed for the complete simulation: Q0 interactive, Q1 standard, Q2 background.
- Interqueue policy is strict priority: Q0 before Q1 before Q2. An arrival in a higher queue preempts a lower-queue job at that tick.
- Q0 uses RR with default quantum 2; Q1 uses RR with default quantum 4; Q2 uses FCFS.
- Within Q0/Q1, arrivals are FIFO and quantum expiry rotates to the tail. A job interrupted by a higher queue returns to the **front** of its unchanged queue and receives a fresh quantum when resumed. A preempted Q2 job likewise remains ahead of later Q2 jobs.
- Strict priority can starve lower queues; this is intentional and contrasted with MLFQ boosts.

### MLFQ

- Three dynamic levels with default quanta `[1, 2, 4]`; all arrivals enter Q0.
- Always run the highest non-empty queue. Within a level, use RR/FIFO.
- A job that consumes its full level quantum without completing is demoted one level (remaining at Q2 if already lowest) and enters that level's tail.
- A new Q0 job preempts work in Q1/Q2. The interrupted job returns to the front of its current level and retains the unused portion of its quantum.
- Every default 20 ticks, before normal selection, all unfinished jobs are boosted to Q0 and their quantum counters reset. Boost ordering is current running job first, followed by ready jobs in their existing queue order from high to low; each sub-order is already deterministic. A boost is a decision boundary and may preempt.
- The model has no I/O, so jobs cannot voluntarily yield to preserve priority. The UI explains that real MLFQ schedulers also use blocking/yield behavior.

MLFQ follows the core rules described in [Operating Systems: Three Easy Pieces](https://pages.cs.wisc.edu/~remzi/OSTEP/cpu-sched-mlfq.pdf): highest priority wins, equal priority uses RR, CPU consumption causes demotion, and periodic boosts prevent starvation.

## Timeline and event semantics

The engine returns both execution slices and an ordered event stream. An `ExecutionSlice` is either a job interval or CPU idle interval; adjacent intervals may be separated to preserve a meaningful quantum/preemption boundary. Every `SimulationEvent` freezes a complete replay snapshot:

- timestamp and monotonically increasing sequence;
- event kind (`JobArrived`, `JobQueued`, `JobSelected`, `JobStarted`, `JobExecuted`, `JobPreempted`, `JobResumed`, `JobCompleted`, `CpuIdle`, `PriorityBoost`);
- subject job where applicable;
- explanation derived by the scheduler;
- ordered ready queue;
- running job;
- completed jobs;
- remaining time per job.

The same-time event order is:

1. execution through the boundary is recorded;
2. completion, quantum expiry, or preemption state transition is recorded;
3. arrivals at the boundary are admitted in global tie order;
4. global actions such as an MLFQ priority boost are recorded;
5. selection and start/resume are recorded.

An algorithm may admit an arrival before recording a cause-specific preemption explanation, but sequence numbers remain authoritative and tests assert each algorithm's externally visible order. The UI consumes snapshots; it never reconstructs ready queues or makes a selection.

## Synchronized comparison replay

Scheduling and replay are separate phases. Every selected algorithm finishes calculating its immutable slices, events, snapshots, and metrics before the comparison is shown. Replaying, pausing, stepping, restarting, or changing speed cannot change a result.

When two or more algorithms are selected:

- a single absolute comparison clock spans time `0` through the greatest selected makespan;
- every algorithm lane uses the same horizontal scale, so equal x-coordinates mean equal ticks;
- “Next shared decision” advances to the next timestamp in the distinct ordered union of all selected event streams;
- at a shared time where one algorithm has no event, its compact debugger shows its most recent snapshot at or before that time;
- the running job is the non-idle execution slice satisfying `StartTime <= sharedTime < EndTime`;
- remaining work is original burst minus all execution accumulated before the shared-time boundary;
- an algorithm that has already reached its makespan remains visibly complete while longer traces continue;
- the optional detailed inspector uses its own event-index playback and does not alter the shared comparison clock.

Before playback starts, the shared display reads time 0 but all algorithm cards explicitly say they are ready to start. This avoids inventing a pre-time-zero tick while preserving a clear untouched state.

## Metrics

Per job:

- `FirstStartTime`: start of its first non-idle execution slice.
- `CompletionTime`: end of its final slice.
- `CpuExecutionTime`: sum of its execution-slice lengths; must equal burst time.
- `TurnaroundTime = CompletionTime - ArrivalTime`.
- `WaitingTime = TurnaroundTime - BurstTime`.
- `ResponseTime = FirstStartTime - ArrivalTime`.

Per algorithm:

- averages are arithmetic means over all jobs;
- `BusyTime` is the sum of non-idle slice lengths;
- `Makespan` is final completion time measured from simulation time 0;
- `CPU utilisation = BusyTime / Makespan × 100`; empty input yields 0;
- `Throughput = completed job count / Makespan` jobs per tick; empty input yields 0;
- `Context switches` count direct job-to-different-job hand-offs at touching slice boundaries. Initial dispatch, same-job quantum renewal, and transitions to/from idle are excluded.

## Validation and resource bounds

- 0 through 100 jobs; empty input returns an empty, zero-metric result.
- Unique IDs, 1–20 characters.
- Arrival: 0–10,000 ticks.
- Burst: 1–1,000 ticks.
- Priority: 1–99.
- Queue level: 0–2.
- Quantum: 1–1,000 ticks.
- MLFQ boost interval: 1–100,000 ticks and strictly greater than zero.
- Sum of bursts: at most 100,000 ticks; calculated with checked arithmetic.

These limits are broad for teaching while preventing accidental browser resource exhaustion and integer overflow.

## Correctness invariants

For every valid non-empty result:

- each job completes exactly once;
- total executed time for each job equals its burst;
- no execution starts before arrival;
- every slice has `End > Start`;
- slices are ordered and never overlap;
- the CPU is fully accounted for from time 0 through makespan by either job or idle slices;
- remaining time never becomes negative;
- completion is not before arrival;
- waiting and response are non-negative;
- repeated scheduling with equal jobs/options/algorithm produces structurally equal slices, events, snapshots, and metrics.

Reference terminology and trade-offs are also cross-checked against the University of Wisconsin [OSTEP scheduling materials](https://pages.cs.wisc.edu/~remzi/OSTEP/Homework/homework.html) and [MLFQ chapter](https://pages.cs.wisc.edu/~remzi/OSTEP/cpu-sched-mlfq.pdf).

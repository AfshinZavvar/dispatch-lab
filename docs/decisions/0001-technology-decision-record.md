# Technology Decision Record

Status: Implemented and verified  
Decision date: 2026-08-15
Last verified: 2026-08-15

## Context

The simulator is a self-contained educational application. It requires a deterministic, reusable scheduling engine, a highly interactive visual replay, component tests, coverage, mutation testing, and a small attack surface. It does not require authentication, persistence, secrets, or external APIs.

## Decisions

| Concern | Decision | Version | Reason | Source |
|---|---|---:|---|---|
| Runtime and target framework | .NET LTS / `net10.0` | SDK 10.0.400; runtime 10.0.11 | .NET 10 is the current active LTS and is supported until November 2028. The pinned SDK is installed in the build environment. | [.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core) |
| Language | C# with `LangVersion=14.0` | 14 | C# 14 is the current stable language supported by .NET 10. Only features that improve clarity will be used. | [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14) |
| UI | Standalone Blazor WebAssembly, SVG, and CSS | ASP.NET Core 10.0.11 | Scheduling and single/shared replay stay in the browser, with no server session or duplicated JavaScript scheduler. Razor components directly render both the focused Gantt view and synchronized multi-lane SVG. The initial payload trade-off is acceptable for a local educational tool. | [Blazor WebAssembly](https://learn.microsoft.com/en-us/aspnet/core/blazor/?view=aspnetcore-10.0), [Razor components and SVG](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/?view=aspnetcore-10.0) |
| JavaScript | No scheduling or chart framework | N/A | SVG and CSS cover the timeline and state transitions. JavaScript is unnecessary for the first release and would add a second state model. |
| Unit test framework | xUnit.net v3 | 3.2.2 | Latest stable v3 package; supports .NET 10. Pre-release xUnit 4 packages are rejected. | [xunit.v3 on NuGet](https://www.nuget.org/packages/xunit.v3/) |
| Test adapter | xUnit Visual Studio runner plus VSTest | 3.1.5 | Stable adapter is retained because Stryker's MTP runner remains preview. It also preserves IDE compatibility. | [xUnit MTP guidance](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform), [runner on NuGet](https://www.nuget.org/packages/xunit.runner.visualstudio/) |
| Test platform | Microsoft.NET.Test.Sdk | 18.9.0 | Current stable test SDK used by the adapter and coverage collector. | [Microsoft.NET.Test.Sdk on NuGet](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk) |
| Test data | AutoFixture plus xUnit v3 integration | AutoFixture.Xunit3 4.19.0 | Stable xUnit v3 integration. Explicit data remains preferred for golden algorithm tests. | [AutoFixture.Xunit3 on NuGet](https://www.nuget.org/packages/AutoFixture.Xunit3) |
| Substitutes | NSubstitute | 6.2.0 | Current stable release, compatible with .NET 10; restricted to orchestration boundaries. Pure schedulers are never mocked. | [NSubstitute on NuGet](https://www.nuget.org/packages/NSubstitute/) |
| AutoFixture/NSubstitute integration | Do not reference AutoFixture.AutoNSubstitute | N/A | Stable 4.18.1 constrains NSubstitute below 6.0 and would conflict with the current stable NSubstitute. Independent use is simpler and warning-free. | [AutoFixture.AutoNSubstitute compatibility](https://www.nuget.org/packages/AutoFixture.AutoNSubstitute/) |
| Component testing | bUnit | 2.9.0 | Current stable Blazor component test library; tests rendered state rather than CSS frames. | [bUnit on NuGet](https://www.nuget.org/packages/bunit/) |
| Coverage | coverlet collector, Cobertura output | 10.0.1 | Current stable VSTest data collector; reports line, branch, and method data consumable without a hosted service. | [coverlet.collector on NuGet](https://www.nuget.org/packages/coverlet.collector/) |
| Mutation testing | Local `dotnet-stryker` tool | 4.16.0 | Pinned .NET mutation tool. Scope is Domain scheduling and metrics; the checked-in configuration uses the preview MTP integration with coverage optimization disabled. Ordinary tests and Coverlet use VSTest. | [Stryker package](https://www.nuget.org/packages/dotnet-stryker), [Stryker configuration](https://stryker-mutator.io/docs/stryker-net/configuration/) |
| Static analysis | SDK analyzers, nullable analysis, warnings as errors, `dotnet format --verify-no-changes` | .NET 10 SDK | Avoids an unnecessary third-party analyzer dependency while enforcing compiler, platform, style, and formatting diagnostics. |
| Vulnerability scanning | `dotnet package list --include-transitive --vulnerable` | .NET 10 CLI | Uses NuGet audit data for direct and transitive packages. A separate deprecated-package check is also run. | [`dotnet package list`](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-package-list) |
| Security baseline | Strong typing, allow-listed ranges, bounded workloads, Razor encoding, no raw HTML | OWASP ASVS 5.0.0 | The app accepts only small numeric/job-label inputs and has no server-side data store. Razor encodes interpolated output; `MarkupString` and unsafe HTML are prohibited. | [OWASP ASVS](https://owasp.org/www-project-application-security-verification-standard/), [ASP.NET Core XSS guidance](https://learn.microsoft.com/en-us/aspnet/core/security/cross-site-scripting) |

## Dependency necessity and maintenance assessment

- Runtime projects use only the .NET/ASP.NET shared framework and the WebAssembly hosting package. There is no database, network client, authentication library, JavaScript framework, charting library, mediator, object mapper, or logging package.
- xUnit, bUnit, Coverlet, AutoFixture, and NSubstitute are test-only assets. AutoFixture is used for general validation/orchestration data; correctness scenarios remain handwritten. NSubstitute is used only where an application service depends on an algorithm catalog boundary.
- Stryker is a repository-local tool so builds do not depend on a machine-global install.
- Every production and test dependency was checked after restore. No vulnerable or deprecated direct/transitive package was reported. Stable maintenance updates were applied to bUnit 2.9.0, Microsoft.NET.Test.Sdk 18.9.0, and NSubstitute 6.2.0; the xUnit 4 prerelease line remains intentionally excluded.

## Rejected alternatives

- **.NET 9:** supported but STS and nearing end of support; no benefit over installed .NET 10 LTS.
- **Preview .NET/C#/xUnit:** higher version numbers do not justify unsupported dependencies. On 2026-08-15, NuGet's outdated JSON reports the xUnit 4 prerelease line as `4.0.0`; the official package page still identifies `3.2.2` as the latest stable `xunit.v3` release, so the preview is intentionally not adopted.
- **Interactive Server rendering:** would add a persistent SignalR session to a completely local simulation and make animation sensitive to connection latency.
- **Large JavaScript visualization framework:** unnecessary for a one-dimensional Gantt timeline and would risk scheduling/replay logic being split across languages.
- **Full browser end-to-end suite:** bUnit state tests are proportionate for a local, dependency-free app; startup smoke verification covers hosting integration.

## Verification outcome

The accepted stack is implemented without an added chart, state-management, or JavaScript scheduling dependency. Restore, warning-free build, automated tests, browser smoke validation, package audits, formatting, coverage, and mutation evidence are recorded in the repository README as they are completed.

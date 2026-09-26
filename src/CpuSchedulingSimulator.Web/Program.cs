using CpuSchedulingSimulator.Application.Algorithms;
using CpuSchedulingSimulator.Application.Simulation;
using CpuSchedulingSimulator.Domain.Algorithms;
using CpuSchedulingSimulator.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<ICpuSchedulingAlgorithm, FirstComeFirstServedAlgorithm>();
builder.Services.AddSingleton<ICpuSchedulingAlgorithm, ShortestJobFirstAlgorithm>();
builder.Services.AddSingleton<ICpuSchedulingAlgorithm, ShortestRemainingTimeFirstAlgorithm>();
builder.Services.AddSingleton<ICpuSchedulingAlgorithm, RoundRobinAlgorithm>();
builder.Services.AddSingleton<ICpuSchedulingAlgorithm, PriorityNonPreemptiveAlgorithm>();
builder.Services.AddSingleton<ICpuSchedulingAlgorithm, PriorityPreemptiveAlgorithm>();
builder.Services.AddSingleton<ICpuSchedulingAlgorithm, HighestResponseRatioNextAlgorithm>();
builder.Services.AddSingleton<ICpuSchedulingAlgorithm, MultiLevelQueueAlgorithm>();
builder.Services.AddSingleton<ICpuSchedulingAlgorithm, MultiLevelFeedbackQueueAlgorithm>();
builder.Services.AddSingleton<IAlgorithmCatalog, AlgorithmCatalog>();
builder.Services.AddSingleton<SimulationRunner>();

await builder.Build().RunAsync();

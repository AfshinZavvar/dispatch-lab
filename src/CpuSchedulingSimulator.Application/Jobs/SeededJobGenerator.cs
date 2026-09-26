using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Application.Jobs;

/// <summary>
/// Produces repeatable teaching workloads from a deterministic seed so demonstrations and tests can be reproduced.
/// </summary>
public static class SeededJobGenerator
{
    public const int DefaultSeed = 20_261_108;
    public const int DefaultJobCount = 5;

    public static IReadOnlyList<JobDefinition> Generate(int seed)
    {
        var random = new Random(seed);
        var priorities = new[] { 1, 2, 3, 4, 5 };
        Shuffle(priorities, random);
        var queues = new[] { 0, 0, 1, 1, 2 };
        Shuffle(queues, random);

        var jobs = new[]
        {
            new JobDefinition("P1", 0, random.Next(8, 12), priorities[0], queues[0]),
            new JobDefinition("P2", 0, random.Next(2, 5), priorities[1], queues[1]),
            new JobDefinition("P3", random.Next(1, 3), random.Next(1, 4), priorities[2], queues[2]),
            new JobDefinition("P4", random.Next(2, 5), random.Next(5, 9), priorities[3], queues[3]),
            new JobDefinition("P5", random.Next(4, 7), random.Next(2, 6), priorities[4], queues[4]),
        };

        return Array.AsReadOnly(jobs);
    }

    private static void Shuffle<T>(T[] items, Random random)
    {
        for (var index = items.Length - 1; index > 0; index--)
        {
            var swapIndex = random.Next(index + 1);
            (items[index], items[swapIndex]) = (items[swapIndex], items[index]);
        }
    }
}

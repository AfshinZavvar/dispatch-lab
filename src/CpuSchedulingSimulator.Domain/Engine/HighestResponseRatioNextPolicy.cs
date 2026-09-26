namespace CpuSchedulingSimulator.Domain.Engine;

/// <summary>
/// Orders ready jobs by descending response ratio using exact integer cross multiplication.
/// It is the non-preemptive engine policy used by <see cref="CpuSchedulingSimulator.Domain.Algorithms.HighestResponseRatioNextAlgorithm"/>.
/// </summary>
internal sealed class HighestResponseRatioNextPolicy : OrderedPolicyBase
{
    public override string ExplainSelection(JobRuntime job, int time)
    {
        var waiting = time - job.Definition.ArrivalTime;
        var ratio = (waiting + job.Definition.BurstTime) / (double)job.Definition.BurstTime;
        return $"{job.Definition.Id} has the highest response ratio: ({waiting} waiting + {job.Definition.BurstTime} burst) / {job.Definition.BurstTime} = {ratio:F2}.";
    }

    protected override int Compare(JobRuntime left, JobRuntime right, int time)
    {
        var leftNumerator = (long)(time - left.Definition.ArrivalTime) + left.Definition.BurstTime;
        var rightNumerator = (long)(time - right.Definition.ArrivalTime) + right.Definition.BurstTime;
        var leftScaled = leftNumerator * right.Definition.BurstTime;
        var rightScaled = rightNumerator * left.Definition.BurstTime;
        var ratioDescending = rightScaled.CompareTo(leftScaled);
        return ratioDescending != 0 ? ratioDescending : CompareGlobalTies(left, right);
    }
}

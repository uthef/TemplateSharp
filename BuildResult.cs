public readonly struct BuildResult
{
    public int TotalCount { get; } = 0;
    public int SkippedCount { get; } = 0;

    public BuildResult(int totalCount, int skippedCount)
    {
        TotalCount = totalCount;
        SkippedCount = skippedCount;
    }
}
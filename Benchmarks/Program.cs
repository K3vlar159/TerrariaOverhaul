using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Common.Dodgerolls;

namespace Benchmarks;

/// <summary>
/// Main entry point pre spustenie benchmarkov.
/// Spustenie: dotnet run -c Release --project Benchmarks/Benchmarks.csproj
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkDotNet.Running.BenchmarkRunner.Run<UtilityBenchmarks>();
    }
}

/// <summary>
/// Benchmarky pre profiling výkonu utility classes.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 5)]
public class UtilityBenchmarks
{
    private Counter counter;
    private Surface<int> surface;
    private DodgerollStats stats;

    [GlobalSetup]
    public void Setup()
    {
        counter = new Counter();
        surface = new Surface<int>(100, 100);
        stats = new DodgerollStats();
    }

    [Benchmark]
    public void Counter_Increase_Dispose()
    {
        using var handle = counter.Increase();
    }

    [Benchmark]
    public void Surface_IndexAccess_10000Times()
    {
        for (int i = 0; i < 100; i++)
        {
            for (int j = 0; j < 100; j++)
            {
                int value = surface[i, j];
            }
        }
    }

    [Benchmark]
    public void Surface_IndexCalculation_10000Times()
    {
        for (int i = 0; i < 100; i++)
        {
            for (int j = 0; j < 100; j++)
            {
                int index = surface.Index(i, j);
            }
        }
    }

    [Benchmark]
    public void DodgerollStats_Initialization()
    {
        var localStats = new DodgerollStats();
        uint charges = localStats.MaxCharges;
        uint cooldown = localStats.CooldownLength;
        uint length = localStats.DodgerollLength;
    }
}

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace ArceusCore.Utils;

public record struct PerformanceMonitor : IDisposable
{
    private readonly ILogger? _logger;
    private int _laps; 
    public PerformanceMonitor(ILogger? logger = null)
    {
        _logger = logger;
        _logger?.LogDebug("{Monitor} started!", nameof(PerformanceMonitor));
        Start = Stopwatch.GetTimestamp();
    }

    [Conditional("DEBUG")]
    public void Reset() => Start = Stopwatch.GetTimestamp();

    public long Start { get; private set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public TimeSpan Elapsed() => Stopwatch.GetElapsedTime(Start);

    public void Dispose()
    {
        var elapsed = Elapsed();
        _logger?.LogDebug("{Monitor} is disposed and was elapsed {TotalMilliseconds}ms", nameof(PerformanceMonitor), elapsed.TotalMilliseconds);
    }

    [Conditional("DEBUG")]
    public void Lap(string name = "unknown")
    {
        var elapsed = Elapsed();
        Reset();
        _logger?.LogDebug("{Monitor} lap {LapName}({Lap}º) was elapsed {TotalMilliseconds}ms", 
            nameof(PerformanceMonitor), 
            name,
            ++_laps, 
            elapsed.TotalMilliseconds
            );
    }
}
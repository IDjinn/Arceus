using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace ArceusCore.Utils;

public record struct PerformanceMonitor : IDisposable
{
    private readonly ILogger? _logger;
    private readonly string _prefix;
    private int _laps;

    public PerformanceMonitor(
        ILogger? logger = null,
        string prefix = nameof(PerformanceMonitor)
    )
    {
        _logger = logger;
        if (prefix.Contains(nameof(PerformanceMonitor)))
            _prefix = prefix;
        else
            _prefix = nameof(PerformanceMonitor) + ' ' + prefix;
        _logger?.LogDebug("{Monitor} started!", prefix);
        Start = Stopwatch.GetTimestamp();
    }

    public void Reset() => Start = Stopwatch.GetTimestamp();
    
    [Conditional("DEBUG")]
    [SuppressMessage("ReSharper", "TemplateIsNotCompileTimeConstantProblem")]
    public void Log([ConstantExpected]string? message, params object?[] args) => _logger?.LogDebug(_prefix + ' ' + message ,args);

    public long Start { get; private set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public TimeSpan Elapsed() => Stopwatch.GetElapsedTime(Start);

    public void Dispose()
    {
        var elapsed = Elapsed();
        _logger?.LogDebug("{Monitor} is disposed and was elapsed {TotalMilliseconds}ms", _prefix, elapsed.TotalMilliseconds);
    }

    [Conditional("DEBUG")]
    public void Lap(string name = "unknown")
    {
        var elapsed = Elapsed();
        Reset();
        _logger?.LogDebug("{Monitor} lap {LapName}({Lap}º) was elapsed {TotalMilliseconds}ms", 
            _prefix, 
            name,
            ++_laps, 
            elapsed.TotalMilliseconds
            );
    }
}
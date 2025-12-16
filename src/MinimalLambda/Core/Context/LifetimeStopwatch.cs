using System.Diagnostics;

namespace MinimalLambda;

internal sealed class LifetimeStopwatch
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    internal TimeSpan Elapsed
    {
        get
        {
            Thread.MemoryBarrier();
            return _stopwatch.Elapsed;
        }
    }
}

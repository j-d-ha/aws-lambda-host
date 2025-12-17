using System.Diagnostics;

namespace MinimalLambda;

internal sealed class LifetimeStopwatch : ILifetimeStopwatch
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    public TimeSpan Elapsed
    {
        get
        {
            Thread.MemoryBarrier();
            return _stopwatch.Elapsed;
        }
    }
}

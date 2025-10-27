using System.Diagnostics;

namespace AwsLambda.Host.Example.OpenTelemetry;

public class Service(Instrumentation instrumentation) : IService
{
    private readonly ActivitySource _activitySource = instrumentation.ActivitySource;

    public async Task<string> GetMessage(string name, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity();

        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

        return $"Hello {name}!";
    }
}

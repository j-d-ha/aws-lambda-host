namespace AwsLambda.Host.Testing;

public class LambdaServerV2 : IAsyncDisposable
{
    private bool _started;
    private bool _stopped;

    public async ValueTask DisposeAsync()
    {
        if (!_stopped)
            await StopAsync();

        // TODO release managed resources here
    }

    public static LambdaServerV2 Create() => new();

    //      ┌──────────────────────────────────────────────────────────┐
    //      │                        Public API                        │
    //      └──────────────────────────────────────────────────────────┘

    public async Task StartAsync(CancellationToken cancellationToken = default) => _started = true;

    public async Task<InvocationResponse<TResponse>> InvokeAsync<TResponse, TEvent>(
        TEvent invokeEvent,
        CancellationToken cancellationToken = default
    )
    {
        if (!_started)
            throw new InvalidOperationException("Server is not started.");

        return default;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_started)
            return;

        _stopped = true;
    }

    //      ┌──────────────────────────────────────────────────────────┐
    //      │                  Internal Server Logic                   │
    //      └──────────────────────────────────────────────────────────┘
}

public static class Temp
{
    public static async Task Run()
    {
        await using var server = LambdaServerV2.Create();
        await server.StartAsync();
        var result = await server.InvokeAsync<string, string>("Jonas", CancellationToken.None);
        await server.StopAsync();
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace AwsLambda.Host;

internal class LambdaLifecycleOrchestrator : ILambdaLifecycleOrchestrator
{
    private readonly DelegateHolder _delegateHolder;
    private readonly IServiceScopeFactory _scopeFactory;

    public LambdaLifecycleOrchestrator(
        IServiceScopeFactory scopeFactory,
        DelegateHolder delegateHolder
    )
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(delegateHolder);

        _scopeFactory = scopeFactory;
        _delegateHolder = delegateHolder;
    }

    public async Task OnShutdown(List<Exception> exceptions, CancellationToken cancellationToken)
    {
        var tasks = new Task<(bool Success, Exception? Error)>[
            _delegateHolder.ShutdownHandlers.Count
        ];

        for (var i = 0; i < _delegateHolder.ShutdownHandlers.Count; i++)
            tasks[i] = RunShutdownHandler(_delegateHolder.ShutdownHandlers[i], cancellationToken);

        var output = await Task.WhenAll(tasks);

        foreach (var (success, error) in output)
            if (!success)
                exceptions.Add(error!);
    }

    private async Task<(bool Success, Exception? Error)> RunShutdownHandler(
        LambdaShutdownDelegate handler,
        CancellationToken cancellationToken
    )
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            await handler(scope.ServiceProvider, cancellationToken);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex);
        }
    }
}

using LanguageExt;
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
        var tasks = _delegateHolder.ShutdownHandlers.Select(h =>
            RunShutdownHandler(h, cancellationToken)
        );

        var output = await Task.WhenAll(tasks);

        exceptions.AddRange(output.Somes());
    }

    private async Task<Option<Exception>> RunShutdownHandler(
        LambdaShutdownDelegate handler,
        CancellationToken cancellationToken
    )
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            await handler(scope.ServiceProvider, cancellationToken);
            return Option<Exception>.None;
        }
        catch (Exception ex)
        {
            return Option<Exception>.Some(ex);
        }
    }
}

using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Lambda.Host.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Lambda.Host;

internal class LambdaHostedService : IHostedService
{
    private readonly ILambdaCancellationTokenSourceFactory _cancellationTokenSourceFactory;
    private readonly DelegateHolder _delegateHolder;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly LambdaHostSettings _settings;

    internal LambdaHostedService(
        IOptions<LambdaHostSettings> lambdaHostSettings,
        DelegateHolder delegateHolder,
        ILambdaCancellationTokenSourceFactory lambdaCancellationTokenSourceFactory,
        IServiceScopeFactory serviceScopeFactory
    )
    {
        _settings =
            lambdaHostSettings.Value ?? throw new ArgumentNullException(nameof(lambdaHostSettings));
        _delegateHolder = delegateHolder ?? throw new ArgumentNullException(nameof(delegateHolder));
        _cancellationTokenSourceFactory =
            lambdaCancellationTokenSourceFactory
            ?? throw new ArgumentNullException(nameof(lambdaCancellationTokenSourceFactory));
        _scopeFactory =
            serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

        if (!_delegateHolder.IsHandlerSet)
            throw new InvalidOperationException("Handler is not set");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var handler = BuildMiddlewarePipeline(
            _delegateHolder.Middlewares,
            _delegateHolder.Handler!
        );

        var wrappedHandler = HandlerWrapper.GetHandlerWrapper(
            async Task<Stream> (Stream inputStream, ILambdaContext lambdaContext) =>
            {
                using var cancellationTokenSource =
                    _cancellationTokenSourceFactory.NewCancellationTokenSource(lambdaContext);

                await using var lambdaHostContext = new LambdaHostContext(
                    lambdaContext,
                    _scopeFactory,
                    cancellationTokenSource.Token,
                    inputStream,
                    _settings.LambdaSerializer
                );

                await handler(lambdaHostContext);

                return lambdaHostContext.OutputStream ?? new MemoryStream(0);
            }
        );

        var bootstrap = _settings.BootstrapHttpClient is null
            ? new LambdaBootstrap(wrappedHandler, _settings.BootstrapOptions, null)
            : new LambdaBootstrap(
                _settings.BootstrapHttpClient,
                wrappedHandler,
                _settings.BootstrapOptions,
                null
            );

        bootstrap.RunAsync(cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static LambdaInvocationDelegate BuildMiddlewarePipeline(
        List<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>> middlewares,
        LambdaInvocationDelegate handler
    )
    {
        var pipeline = handler;

        for (var i = middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = middlewares[i];
            pipeline = middleware(pipeline);
        }

        return pipeline;
    }
}

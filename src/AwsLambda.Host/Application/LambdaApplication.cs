using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AwsLambda.Host;

/// <summary>A Lambda application that provides host functionality for running AWS Lambda handlers.</summary>
public sealed class LambdaApplication
    : IHost,
        ILambdaInvocationBuilder,
        ILambdaOnInitBuilder,
        ILambdaOnShutdownBuilder,
        IAsyncDisposable
{
    private readonly IHost _host;
    private readonly ILambdaInvocationBuilder _invocationBuilder;
    private readonly ILambdaOnInitBuilder _onInitBuilder;
    private readonly ILambdaOnShutdownBuilder _onShutdownBuilder;
    private IReadOnlyList<LambdaInitDelegate> _initHandlers;

    internal LambdaApplication(IHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        _host = host;

        _invocationBuilder = Services
            .GetRequiredService<IInvocationBuilderFactory>()
            .CreateBuilder();

        _onInitBuilder = Services.GetRequiredService<IOnInitBuilderFactory>().CreateBuilder();

        _onShutdownBuilder = Services
            .GetRequiredService<IOnShutdownBuilderFactory>()
            .CreateBuilder();
    }

    public IConfiguration Configuration =>
        field ??= _host.Services.GetRequiredService<IConfiguration>();

    public IHostEnvironment Environment =>
        field ??= _host.Services.GetRequiredService<IHostEnvironment>();

    public IHostApplicationLifetime Lifetime =>
        field ??= _host.Services.GetRequiredService<IHostApplicationLifetime>();

    public ILogger Logger =>
        field ??=
            _host
                .Services.GetService<ILoggerFactory>()
                ?.CreateLogger(Environment.ApplicationName ?? nameof(LambdaApplication))
            ?? NullLogger.Instance;

    /// <inheritdoc />
    public ValueTask DisposeAsync() => ((IAsyncDisposable)_host).DisposeAsync();

    public IServiceProvider Services => _host.Services;

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken = default) =>
        _host.StartAsync(cancellationToken);

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken = default) =>
        _host.StopAsync(cancellationToken);

    /// <inheritdoc />
    public void Dispose() => _host.Dispose();

    //      ┌──────────────────────────────────────────────────────────┐
    //      │                 ILambdaInvocationBuilder                 │
    //      └──────────────────────────────────────────────────────────┘

    /// <inheritdoc />
    public IDictionary<string, object?> Properties => _invocationBuilder.Properties;

    /// <inheritdoc />
    public IList<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>> Middlewares =>
        _invocationBuilder.Middlewares;

    /// <inheritdoc />
    public LambdaInvocationDelegate? Handler => _invocationBuilder.Handler;

    /// <inheritdoc />
    public ILambdaInvocationBuilder Handle(LambdaInvocationDelegate handler)
    {
        _invocationBuilder.Handle(handler);
        return this;
    }

    /// <inheritdoc />
    public ILambdaInvocationBuilder Use(
        Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware
    )
    {
        _invocationBuilder.Use(middleware);
        return this;
    }

    /// <inheritdoc />
    public LambdaInvocationDelegate Build() => _invocationBuilder.Build();

    //      ┌──────────────────────────────────────────────────────────┐
    //      │                   ILambdaOnInitBuilder                   │
    //      └──────────────────────────────────────────────────────────┘

    /// <inheritdoc />
    public IReadOnlyList<LambdaInitDelegate> InitHandlers => _onInitBuilder.InitHandlers;

    /// <inheritdoc />
    public ILambdaOnInitBuilder OnInit(LambdaInitDelegate handler)
    {
        _onInitBuilder.OnInit(handler);
        return this;
    }

    /// <inheritdoc />
    LambdaInitDelegate ILambdaOnInitBuilder.Build() => _onInitBuilder.Build();

    //      ┌──────────────────────────────────────────────────────────┐
    //      │                 ILambdaOnShutdownBuilder                 │
    //      └──────────────────────────────────────────────────────────┘

    /// <inheritdoc />
    public IList<LambdaShutdownDelegate> ShutdownHandlers => _onShutdownBuilder.ShutdownHandlers;
}

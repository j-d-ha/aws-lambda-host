using Microsoft.Extensions.Hosting;

namespace AwsLambda.Host;

/// <summary>
///     Orchestrates the Lambda hosting environment lifecycle.
///     Delegates specific concerns to specialized components.
/// </summary>
internal sealed class LambdaHostedService : BackgroundService
{
    private readonly ILambdaBootstrapOrchestrator _bootstrap;
    private readonly List<Exception> _exceptions = [];
    private readonly ILambdaHandlerFactory _handlerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="LambdaHostedService"/> class.
    /// </summary>
    /// <param name="bootstrap">The orchestrator responsible for managing the AWS Lambda bootstrap loop.</param>
    /// <param name="handlerFactory">The factory responsible for creating and composing the Lambda request handler.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="bootstrap"/> or <paramref name="handlerFactory"/> is null.</exception>
    public LambdaHostedService(
        ILambdaBootstrapOrchestrator bootstrap,
        ILambdaHandlerFactory handlerFactory
    )
    {
        ArgumentNullException.ThrowIfNull(bootstrap);
        ArgumentNullException.ThrowIfNull(handlerFactory);

        _bootstrap = bootstrap;
        _handlerFactory = handlerFactory;
    }

    /// <summary>
    /// Executes the Lambda hosting environment startup sequence.
    /// </summary>
    /// <remarks>
    /// This method orchestrates the startup of the Lambda service by:
    /// 1. Creating a fully composed handler with middleware pipeline and request processing
    /// 2. Running the AWS Lambda bootstrap loop with the composed handler
    /// The bootstrap loop continues until the service is stopped or an exception occurs.
    /// </remarks>
    /// <param name="stoppingToken">The cancellation token triggered when the service is shutting down.</param>
    /// <returns>A task representing the asynchronous bootstrap operation.</returns>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Create a fully composed handler with middleware and request processing.
        var requestHandler = _handlerFactory.CreateHandler(stoppingToken);

        // Run the bootstrap with the processed handler.
        return _bootstrap.RunAsync(requestHandler, stoppingToken);
    }

    /// <summary>
    /// Stops the Lambda hosting environment and handles any exceptions that occurred during shutdown.
    /// </summary>
    /// <remarks>
    /// This method aggregates any exceptions that occur during the base shutdown operation and
    /// rethrows them as an <see cref="AggregateException"/> if any were captured. This ensures that
    /// shutdown failures are properly reported rather than silently ignored.
    /// </remarks>
    /// <param name="cancellationToken">The cancellation token for stopping the service.</param>
    /// <returns>A task representing the asynchronous stop operation.</returns>
    /// <exception cref="AggregateException">Thrown when one or more exceptions occurred during shutdown.</exception>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        // await the background service stop and capture any exceptions that occur.
        try
        {
            await base.StopAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _exceptions.Add(ex);
        }

        // if any exceptions were captured, rethrow them.
        if (_exceptions.Count > 0)
            throw new AggregateException(_exceptions);
    }
}

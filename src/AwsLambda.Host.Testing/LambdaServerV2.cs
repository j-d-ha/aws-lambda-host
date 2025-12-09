using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;

namespace AwsLambda.Host.Testing;

public class LambdaServerV2 : IAsyncDisposable
{
    private readonly TaskCompletionSource<InitResponse> _initCompletionTcs;
    private readonly SemaphoreSlim _invocationAddedSignal;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ConcurrentQueue<string> _pendingInvocationIds;
    private readonly ConcurrentDictionary<string, PendingInvocation> _pendingInvocations;
    private readonly ILambdaRuntimeRouteManager _routeManager;
    private readonly CancellationTokenSource _shutdownCts;
    private readonly Channel<LambdaHttpTransaction> _transactionChannel;
    private readonly Task<Exception?> _entryPointCompletion;

    private IHost? _host;

    private Task? _processingTask;
    private ServerState _state;

    internal LambdaServerV2(
        Task<Exception?>? entryPointCompletion,
        CancellationToken shutdownToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entryPointCompletion);

        _entryPointCompletion = entryPointCompletion;

        _transactionChannel = Channel.CreateUnbounded<LambdaHttpTransaction>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = false }
        );
        _pendingInvocationIds = new ConcurrentQueue<string>();
        _pendingInvocations = new ConcurrentDictionary<string, PendingInvocation>();
        _routeManager = new LambdaRuntimeRouteManager();
        _jsonSerializerOptions = new JsonSerializerOptions();
        _shutdownCts = CancellationTokenSource.CreateLinkedTokenSource(shutdownToken);
        _initCompletionTcs = new TaskCompletionSource<InitResponse>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        _invocationAddedSignal = new SemaphoreSlim(0);
        _state = ServerState.Created;
    }

    internal void SetHost(IHost host)
    {
        ArgumentNullException.ThrowIfNull(host);
        _host = host;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();

        _transactionChannel.Writer.TryComplete();

        _state = ServerState.Disposed;
    }

    public IServiceProvider Services => _host.Services;

    internal HttpMessageHandler CreateHandler() =>
        new LambdaTestingHttpHandler(_transactionChannel);

    //      ┌──────────────────────────────────────────────────────────┐
    //      │                        Public API                        │
    //      └──────────────────────────────────────────────────────────┘

    public async Task<InitResponse> StartAsync(CancellationToken cancellationToken = default)
    {
        if (_state != ServerState.Created)
            throw new InvalidOperationException("Server is already started.");

        if (_host is null)
            throw new InvalidOperationException("Host is not set.");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _state = ServerState.Starting;

        // Start the host
        await _host.StartAsync(cts.Token);

        // Start background processing
        _processingTask = Task.Run(ProcessTransactionsAsync, cts.Token);

        var exceptions = await WhenAny(
            _processingTask,
            _entryPointCompletion,
            _initCompletionTcs.Task
        );

        if (exceptions.Length > 0)
            throw exceptions.Length > 0
                ? new AggregateException(
                    "Multiple exceptions encountered while running StartAsync",
                    exceptions
                )
                : exceptions[0];

        if (_entryPointCompletion.IsCompleted)
            return new InitResponse { InitStatus = InitStatus.HostExited };

        if (_initCompletionTcs.Task.IsCompleted)
        {
            _state =
                _initCompletionTcs.Task.Result.InitStatus == InitStatus.InitCompleted
                    ? ServerState.Running
                    : ServerState.Stopped;

            return _initCompletionTcs.Task.Result;
        }

        throw new InvalidOperationException(
            "Server initialization failed with neither an error nor completion."
        );
    }

    private static async Task<Exception[]> WhenAny(params Task[] tasks)
    {
        await Task.WhenAny(tasks);
        return ExtractExceptions(tasks);
    }

    private static async Task<Exception[]> WhenAll(params Task[] tasks)
    {
        await Task.WhenAll(tasks);
        return ExtractExceptions(tasks);
    }

    private static Exception[] ExtractExceptions(Task[] tasks) =>
        tasks
            .Where(t => t is { IsFaulted: true, Exception: not null })
            .Select(e =>
                e.Exception!.InnerExceptions.Count > 1
                    ? e.Exception
                    : e.Exception.InnerExceptions[0]
            )
            .ToArray();

    public async Task<InvocationResponse<TResponse>> InvokeAsync<TResponse, TEvent>(
        TEvent invokeEvent,
        CancellationToken cancellationToken = default
    )
    {
        if (_state != ServerState.Running)
            throw new InvalidOperationException("Server is not started.");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var pending = PendingInvocation.Create(requestId, eventResponse, deadlineUtc);

        return default;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_state != ServerState.Running)
            return;

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _state = ServerState.Stopped;
    }

    //      ┌──────────────────────────────────────────────────────────┐
    //      │                  Internal Server Logic                   │
    //      └──────────────────────────────────────────────────────────┘

    private async Task ProcessTransactionsAsync()
    {
        await foreach (
            var transaction in _transactionChannel.Reader.ReadAllAsync(_shutdownCts.Token)
        )
            try
            {
                if (
                    !_routeManager.TryMatch(
                        transaction.Request,
                        out var requestType,
                        out var routeValues
                    )
                )
                    throw new InvalidOperationException(
                        $"Unexpected request: {transaction.Request.Method} {transaction.Request.RequestUri}"
                    );

                switch (requestType!.Value)
                {
                    case RequestType.GetNextInvocation:
                        await HandleGetNextInvocationAsync(transaction);
                        break;

                    case RequestType.PostResponse:
                        await HandlePostResponseAsync(transaction, routeValues!);
                        break;

                    case RequestType.PostError:
                        await HandlePostErrorAsync(transaction, routeValues!);
                        break;

                    case RequestType.PostInitError:
                        await HandlePostInitErrorAsync(transaction);
                        break;

                    default:
                        throw new InvalidOperationException(
                            $"Unexpected request type {requestType} for {transaction.Request.RequestUri}"
                        );
                }
            }
            catch (Exception ex)
            {
                // Fail the transaction and continue processing
                // transaction.Fail(ex);
                throw;
            }
    }

    private async Task HandleGetNextInvocationAsync(LambdaHttpTransaction transaction)
    {
        if (_state == ServerState.Starting)
        {
            _initCompletionTcs.SetResult(
                new InitResponse { InitStatus = InitStatus.InitCompleted }
            );
            return;
        }
    }

    private async Task HandlePostResponseAsync(
        LambdaHttpTransaction transaction,
        RouteValueDictionary routeValues
    ) { }

    private async Task HandlePostErrorAsync(
        LambdaHttpTransaction transaction,
        RouteValueDictionary routeValues
    ) { }

    private async Task HandlePostInitErrorAsync(LambdaHttpTransaction transaction)
    {
        if (_state == ServerState.Starting)
            _initCompletionTcs.SetResult(
                new InitResponse
                {
                    Error = await (
                        transaction.Request.Content?.ReadFromJsonAsync<ErrorResponse>(
                            _jsonSerializerOptions
                        ) ?? Task.FromResult<ErrorResponse?>(null)
                    ),
                    InitStatus = InitStatus.InitError,
                }
            );

        throw new InvalidOperationException(
            "Server is already started and as such an initialization error cannot be reported."
        );
    }
}

// public static class Temp
// {
//     public static async Task Run()
//     {
//         await using var server = new LambdaServerV2();
//         await server.StartAsync();
//         var result = await server.InvokeAsync<string, string>("Jonas", CancellationToken.None);
//         await server.StopAsync();
//     }
// }

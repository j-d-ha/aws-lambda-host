namespace MinimalLambda.Testing;

public static class LambdaTestServerExtensions
{
    extension(LambdaTestServer server)
    {
        public Task<InvocationResponse<TResponse>> InvokeAsync<TResponse, TEvent>(
            TEvent invokeEvent,
            CancellationToken cancellationToken = default
        ) =>
            server.InvokeAsync<TResponse, TEvent>(
                invokeEvent,
                cancellationToken: cancellationToken
            );

        public Task<InvocationResponse<TResponse>> InvokeNoEventAsync<TResponse>(
            CancellationToken cancellationToken = default
        ) => server.InvokeAsync<TResponse, object>(null, cancellationToken: cancellationToken);

        public async Task<InvocationResponse> InvokeNoResponseAsync<TEvent>(
            TEvent invokeEvent,
            CancellationToken cancellationToken = default
        ) =>
            await server.InvokeAsync<object, TEvent>(
                invokeEvent,
                true,
                cancellationToken: cancellationToken
            );
    }
}

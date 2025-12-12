namespace MinimalLambda.Testing;

public static class LambdaTestServerExtensions
{
    extension(LambdaTestServer server)
    {
        public Task<InvocationResponse<TResponse>> InvokeAsync<TEvent, TResponse>(
            TEvent invokeEvent,
            CancellationToken cancellationToken = default
        ) =>
            server.InvokeAsync<TEvent, TResponse>(
                invokeEvent,
                false,
                cancellationToken: cancellationToken
            );

        public Task<InvocationResponse<TResponse>> InvokeNoEventAsync<TResponse>(
            CancellationToken cancellationToken = default
        ) =>
            server.InvokeAsync<object, TResponse>(
                null,
                false,
                cancellationToken: cancellationToken
            );

        public async Task<InvocationResponse> InvokeNoResponseAsync<TEvent>(
            TEvent invokeEvent,
            CancellationToken cancellationToken = default
        ) =>
            await server.InvokeAsync<TEvent, object>(
                invokeEvent,
                true,
                cancellationToken: cancellationToken
            );
    }
}

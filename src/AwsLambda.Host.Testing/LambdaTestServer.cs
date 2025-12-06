using System.Text.Json;
using System.Threading.Channels;

namespace AwsLambda.Host.Testing;

internal class LambdaTestServer : IDisposable
{
    private readonly Channel<HttpRequestMessage> _requestChanel;
    private readonly Channel<HttpResponseMessage> _responseChanel;

    public LambdaTestServer()
    {
        _requestChanel = Channel.CreateUnbounded<HttpRequestMessage>();
        _responseChanel = Channel.CreateUnbounded<HttpResponseMessage>();
    }

    internal HttpMessageHandler CreateTestingHandler() =>
        new LambdaTestingHttpHandler(_requestChanel, _responseChanel);

    internal LambdaClient CreateLambdaClient(
        JsonSerializerOptions jsonSerializerOptions,
        ILambdaRuntimeRouteManager routeManager
    ) => new(_requestChanel, _responseChanel, jsonSerializerOptions, routeManager);

    public void Dispose() { }
}

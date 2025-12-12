namespace MinimalLambda.Testing.UnitTests;

public class NoResponseLambdaTests : IClassFixture<LambdaApplicationFactory<NoResponseLambda>>
{
    private readonly LambdaTestServer _server;

    public NoResponseLambdaTests(LambdaApplicationFactory<NoResponseLambda> factory)
    {
        factory.WithCancelationToken(TestContext.Current.CancellationToken);
        _server = factory.TestServer;
    }

    [Fact]
    public async Task NoResponseLambda_ReturnsExpectedValue()
    {
        var response = await _server.InvokeNoResponseAsync<NoResponseLambdaRequest>(
            new NoResponseLambdaRequest("World"),
            TestContext.Current.CancellationToken
        );

        response.WasSuccess.Should().BeTrue();
        response.Should().NotBeNull();
    }
}

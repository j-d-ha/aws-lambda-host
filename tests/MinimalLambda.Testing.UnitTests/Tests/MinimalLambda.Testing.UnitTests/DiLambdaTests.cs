namespace MinimalLambda.Testing.UnitTests;

public class DiLambdaTests : IClassFixture<LambdaApplicationFactory<DiLambda>>
{
    private readonly LambdaTestServer _server;

    public DiLambdaTests(LambdaApplicationFactory<DiLambda> factory)
    {
        factory.WithCancelationToken(TestContext.Current.CancellationToken);
        _server = factory.TestServer;
    }

    [Fact]
    public async Task NoEvent_ReturnsExpectedValue()
    {
        var response = await _server.InvokeAsync<DiLambdaRequest, DiLambdaResponse>(
            new DiLambdaRequest("World"),
            TestContext.Current.CancellationToken
        );

        response.Should().NotBeNull();
        response.WasSuccess.Should().BeTrue();
        response.Response.Should().NotBeNull();
        response.Response.Message.Should().Be("Hello World!");
    }
}

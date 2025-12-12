namespace MinimalLambda.Testing.UnitTests;

public class SimpleLambdaTests
{
    [Fact]
    public async Task SimpleLambda_ReturnsExpectedValue()
    {
        await using var factory = new LambdaApplicationFactory<SimpleLambda>().WithCancelationToken(
            TestContext.Current.CancellationToken
        );
        var setup = await factory.TestServer.StartAsync(TestContext.Current.CancellationToken);
        setup.InitStatus.Should().Be(InitStatus.InitCompleted);

        var response = await factory.TestServer.InvokeAsync<string, string>(
            "World",
            cancellationToken: TestContext.Current.CancellationToken
        );

        response.WasSuccess.Should().BeTrue();
        response.Should().NotBeNull();
        response.Response.Should().Be("Hello World!");
    }
}

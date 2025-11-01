namespace AwsLambda.Host.SourceGenerators.UnitTests;

public class OnShutdownVerifyTests
{
    [Fact]
    public async Task Test_OnShutdown_BaseMethodCall() =>
        await GeneratorTestHelpers.Verify(
            """
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.OnShutdown(
                async (services, token) =>
                {
                    return;
                }
            );

            await lambda.RunAsync();
            """,
            0
        );

    [Fact]
    public async Task Test_OnShutdown_NoInput() =>
        await GeneratorTestHelpers.Verify(
            """
            using System.Threading.Tasks;
            using AwsLambda.Host;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();

            var lambda = builder.Build();

            lambda.OnShutdown(Task () =>
            {
                return Task.CompletedTask;
            });

            await lambda.RunAsync();
            """,
            0
        );
}

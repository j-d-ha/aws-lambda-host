namespace AwsLambda.Host.SourceGenerators.UnitTests;

public class KeyedServiceVerifyTests
{
    [Fact]
    public async Task Test_KeyedService_() =>
        await GeneratorTestHelpers.Verify(
            """

            """
        );

    [Fact]
    public async Task Test_KeyedService_StringKey() =>
        await GeneratorTestHelpers.Verify(
            """

            """
        );

    [Fact]
    public async Task Test_KeyedService_EnumKey() =>
        await GeneratorTestHelpers.Verify(
            """
            using AwsLambda.Host;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var builder = LambdaApplication.CreateBuilder();
            builder.Services.AddKeyedSingleton<IService, Service>(DatabaseType.Secondary);

            var lambda = builder.Build();

            lambda.MapHandler(
                ([FromKeyedServices(DatabaseType.Secondary)] IService service) => service.GetMessage()
            );

            await lambda.RunAsync();

            public enum DatabaseType
            {
                Primary,
                Secondary,
                ReadOnly,
            }

            public interface IService
            {
                string GetMessage();
            }

            public class Service : IService
            {
                public string GetMessage() => "Hello world";
            }
            """
        );
}

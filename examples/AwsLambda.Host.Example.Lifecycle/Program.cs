using System;
using System.Threading;
using System.Threading.Tasks;
using AwsLambda.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = LambdaApplication.CreateBuilder();

builder.Services.ConfigureLambdaHost(options =>
{
    options.RuntimeShutdownDuration = TimeSpan.FromSeconds(3);
    options.RuntimeShutdownDurationBuffer = TimeSpan.FromSeconds(1);
});

builder.Services.AddSingleton<IService, Service>();

var lambda = builder.Build();

lambda.UseClearLambdaOutputFormatting();

lambda.MapHandler(() => new Response("Hello world"));

lambda.OnShutdown(
    async (services, token) =>
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("1 Shutting down...");
        await Task.Delay(TimeSpan.FromSeconds(1), token);
    }
);

lambda.OnShutdown(
    Task (IService service, CancellationToken token) =>
    {
        Console.WriteLine(service.GetMessage());
        return Task.CompletedTask;
    }
);

await lambda.RunAsync();

internal record Response(string Message);

public interface IService
{
    string GetMessage();
}

public class Service : IService
{
    public string GetMessage() => "Hello world";
}

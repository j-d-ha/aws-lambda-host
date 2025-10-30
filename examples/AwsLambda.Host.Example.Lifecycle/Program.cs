using AwsLambda.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = LambdaApplication.CreateBuilder();

builder.Services.Configure<HostOptions>(options =>
{
    options.ShutdownTimeout = TimeSpan.FromSeconds(2);
});

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
    async (services, token) =>
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("2 Shutting down...");
        await Task.Delay(TimeSpan.FromSeconds(1), token);
    }
);

lambda.OnShutdown(
    async (services, token) =>
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        var counter = 0;
        while (!token.IsCancellationRequested)
        {
            logger.LogInformation("Loop {counter}", counter++);
            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }
    }
);

await lambda.RunAsync();

internal record Response(string Message);

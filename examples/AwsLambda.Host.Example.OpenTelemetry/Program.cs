using AwsLambda.Host;
using AwsLambda.Host.Example.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AWSLambda;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = LambdaApplication.CreateBuilder();

builder.Services.AddScoped<IService, Service>();
builder.Services.AddSingleton<Instrumentation>();

builder
    .Services.AddOpenTelemetry()
    .WithTracing(configure =>
        configure
            .AddAWSLambdaConfigurations()
            .AddSource("MyLambda")
            .SetResourceBuilder(
                ResourceBuilder
                    .CreateDefault()
                    .AddService(serviceName: "MyLambda", serviceVersion: "1.0.0")
            )
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri("http://localhost:4318/v1/traces");
                options.Protocol = OtlpExportProtocol.HttpProtobuf;
            })
    );

var lambda = builder.Build();

lambda.UseOpenTelemetryTracing();

lambda.MapHandler(
    async (
        [Event] Request request,
        IService service,
        Instrumentation instrumentation,
        CancellationToken cancellationToken
    ) =>
    {
        // Name need to be passed to StartActivity or the span name will be `<Main>$`.
        // This is because the handler is a lambda expression.
        using var activity = instrumentation.ActivitySource.StartActivity("Handler");
        var message = await service.GetMessage(request.Name, cancellationToken);

        return new Response(message, DateTime.UtcNow);
    }
);

await lambda.RunAsync();

record Request(string Name);

record Response(string Message, DateTime Timestamp);

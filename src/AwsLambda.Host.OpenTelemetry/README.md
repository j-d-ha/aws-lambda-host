# AwsLambda.Host.OpenTelemetry

Automatic distributed tracing for AWS Lambda functions using OpenTelemetry, powered by compile-time
code generation and C# 11 interceptors.

> ⚠️ **Development Status**: This project is actively under development and not yet
> production-ready. Breaking changes may occur in future versions. Use at your own discretion in
> production environments.

> **Requirements**: .NET 9 or later

## Overview

**AwsLambda.Host.OpenTelemetry** provides seamless integration of OpenTelemetry distributed tracing
with AWS Lambda handlers. Using C# 11 interceptors and source code generation, tracing is
automatically injected at compile time with zero runtime overhead.

Enable tracing with a single method call:

```csharp
lambda.UseOpenTelemetryTracing();
```

That's it. All Lambda invocations are now automatically traced to your configured OpenTelemetry
backend (CloudWatch, Jaeger, OTLP, etc.).

> **Note:** The framework automatically traces the Lambda invocation itself (start, duration,
> context). However, method calls and operations within your handler logic may still require
> additional instrumentation depending on your needs. For example, AWS SDK calls are automatically
> traced with `AddAWSLambdaConfigurations()`, but custom business logic or third-party libraries may
> need explicit instrumentation using `Activity.Current` or additional instrumentations.

## Key Features

- **One-Line Setup** – Enable tracing with `UseOpenTelemetryTracing()`
- **Compile-Time Instrumentation** – Uses C# 11 interceptors for zero runtime overhead
- **Automatic Handler Detection** – Supports all handler signatures (event+response, event-only,
  response-only, void)
- **Full Context Propagation** – Lambda context, request IDs, and distributed trace IDs
  automatically included
- **Flexible Export** – Works with CloudWatch, Jaeger, OTLP, Console, and custom exporters
- **AOT Compatible** – Full support for ahead-of-time compilation
- **Best Practices** – Follows OpenTelemetry conventions and AWS Lambda observability patterns

## Installation

Install the NuGet package:

```bash
dotnet add package AwsLambda.Host.OpenTelemetry
```

Also install the AWS Lambda instrumentation package (required for `AddAWSLambdaConfigurations()`):

```bash
dotnet add package OpenTelemetry.Instrumentation.AWSLambda
```

Then install OpenTelemetry exporter packages for your backend:

```bash
# For CloudWatch (recommended for AWS Lambda)
dotnet add package OpenTelemetry.Exporter.CloudTrace

# For Jaeger (for local development)
dotnet add package OpenTelemetry.Exporter.Jaeger

# For OTLP (generic backend)
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol
```

## Quick Start

### Basic Setup

```csharp
using AwsLambda.Host;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

var builder = LambdaApplication.CreateBuilder();

// Register OpenTelemetry with AWS Lambda configurations
builder.Services
    .AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAWSLambdaConfigurations()
        .AddConsoleExporter());  // For development

var lambda = builder.Build();

// Enable automatic tracing
lambda.UseOpenTelemetryTracing();

// Define your handler
lambda.MapHandler(([Event] string input) =>
{
    return $"Received: {input}";
});

await lambda.RunAsync();
```

All Lambda invocations are now automatically traced with:

- Invocation start and end timestamps
- Total execution duration
- Lambda context information (request ID, function name, memory, etc.)
- Exception details (if any)
- Distributed trace correlation IDs

### CloudWatch Integration (Production)

```csharp
using AwsLambda.Host;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

var builder = LambdaApplication.CreateBuilder();

builder.Services
    .AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAWSLambdaConfigurations()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:4317");  // For CloudWatch
        }));

var lambda = builder.Build();
lambda.UseOpenTelemetryTracing();
lambda.MapHandler(([Event] Request request, ILogger<Program> logger) =>
{
    logger.LogInformation("Processing request: {RequestId}", request.Id);
    return new Response { Message = "OK" };
});

await lambda.RunAsync();
```

## Handler Signature Support

Source generation automatically detects and instruments all handler signatures. Whether your handler
accepts events, returns responses, is async, or handles any combination of inputs and outputs—the
framework handles it seamlessly. You don't need to worry about signature compatibility; tracing
works with whatever handler signature you define.

## Advanced Usage

### Adding Custom Attributes to Spans

You can add custom attributes to the trace span through dependency injection:

```csharp
using AwsLambda.Host;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

var builder = LambdaApplication.CreateBuilder();

builder.Services
    .AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAWSLambdaConfigurations()
        .AddConsoleExporter());

var lambda = builder.Build();
lambda.UseOpenTelemetryTracing();

lambda.MapHandler(([Event] Request request, IServiceProvider provider) =>
{
    var tracer = provider.GetTracer<Request, Response>();

    // Get the current activity and add custom attributes
    System.Diagnostics.Activity.Current?.SetTag("request.id", request.Id);
    System.Diagnostics.Activity.Current?.SetTag("custom.field", "value");

    return new Response { Message = "OK" };
});

await lambda.RunAsync();
```

### Using with Middleware

Combine OpenTelemetry tracing with middleware for comprehensive observability:

```csharp
using AwsLambda.Host;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

var builder = LambdaApplication.CreateBuilder();

builder.Services
    .AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAWSLambdaConfigurations()
        .AddConsoleExporter());

var lambda = builder.Build();

// Add middleware for logging and error handling
lambda.Use(async (context, next) =>
{
    var logger = context.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Request started");
        await next();
        logger.LogInformation("Request completed");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Request failed");
        throw;
    }
});

lambda.UseOpenTelemetryTracing();

lambda.MapHandler(([Event] Request request) => new Response { Message = "OK" });

await lambda.RunAsync();
```

### With Dependency Injection

Inject services into your handler and trace them:

```csharp
public interface IOrderService
{
    Task<Order> ProcessAsync(string orderId);
}

public class OrderService : IOrderService
{
    private readonly ILogger<OrderService> _logger;

    public OrderService(ILogger<OrderService> logger)
    {
        _logger = logger;
    }

    public async Task<Order> ProcessAsync(string orderId)
    {
        _logger.LogInformation("Processing order {OrderId}", orderId);
        // Processing logic here
        return new Order { Id = orderId, Status = "Processed" };
    }
}

// In Lambda setup
var builder = LambdaApplication.CreateBuilder();

builder.Services
    .AddScoped<IOrderService, OrderService>()
    .AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAWSLambdaConfigurations()
        .AddConsoleExporter());

var lambda = builder.Build();
lambda.UseOpenTelemetryTracing();

lambda.MapHandler(([Event] string orderId, IOrderService service) =>
{
    return service.ProcessAsync(orderId);
});

await lambda.RunAsync();
```

## Configuration

The `AddAWSLambdaConfigurations()` method handles Lambda-specific setup automatically:

```csharp
builder.Services
    .AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAWSLambdaConfigurations()
        .AddConsoleExporter());  // Choose your exporter
```

This configures service naming, Lambda context attributes, AWS SDK instrumentation, and exception
recording.

For more advanced configuration options (samplers, propagators, custom exporters, etc.), refer to
the [OpenTelemetry .NET documentation](https://opentelemetry.io/docs/instrumentation/net/)
and [AWS Distro for OpenTelemetry](https://aws-otel.github.io/docs/getting-started/lambda).

## What Gets Traced

OpenTelemetry automatically captures:

- **Lambda Invocation Span**
  - Start time
  - Duration
  - Lambda context (request ID, function name, memory, etc.)
  - Event and response payloads
  - Distributed trace IDs

- **Error Information**
  - Exception type and message
  - Stack trace
  - Error status on span

- **Custom Attributes** (via Activity.Current)
  - User-defined attributes
  - Business context
  - Request tracking IDs

- **AWS SDK Calls** (with AddAWSLambdaConfigurations)
  - S3, DynamoDB, SQS, SNS, etc.
  - Call duration and parameters
  - Success/failure status

## API Reference

### Extension Methods

#### `LambdaApplicationExtensions.UseOpenTelemetryTracing()`

Enables automatic tracing for all registered handlers.

```csharp
lambda.UseOpenTelemetryTracing();
```

**Parameters:** None

**Returns:** `ILambdaApplication` (for method chaining)

**Notes:**

- Must be called after `builder.Build()`
- Must be called before `MapHandler()`
- Handler-level tracing is injected at compile time

#### `IServiceProvider.GetTracer<TEvent, TResponse>()`

Manually retrieve the tracer for a specific handler signature.

```csharp
var tracer = serviceProvider.GetTracer<Request, Response>();
```

**Type Parameters:**

- `TEvent` - Event type
- `TResponse` - Response type

**Returns:** `ActivitySource` for manual span creation

**Available Overloads:**

- `GetTracer<TEvent, TResponse>()` - Event and response
- `GetTracerNoResponse<TEvent>()` - Event only
- `GetTracerNoEvent<TResponse>()` - Response only
- `GetTracerNoEventNoResponse()` - No event or response

## Best Practices

### 1. Always Enable for Production

Tracing should be enabled in all environments, especially production:

```csharp
var builder = LambdaApplication.CreateBuilder();

// Always register OpenTelemetry
builder.Services
    .AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAWSLambdaConfigurations()
        .AddOtlpExporter(options => { /* configure */ }));

var lambda = builder.Build();

// Always enable tracing
lambda.UseOpenTelemetryTracing();
```

### 2. Use Scoped Services with Tracing

Scoped services are properly managed per Lambda invocation. Combine them with tracing:

```csharp
// Register as Scoped - one instance per invocation
builder.Services.AddScoped<IRequestContext, RequestContext>();

// Scoped instance will be properly traced
lambda.MapHandler(([Event] Request req, IRequestContext context) =>
{
    // context is unique to this invocation
    return ProcessRequest(req, context);
});
```

### 3. Add Business Context to Spans

Use `Activity.Current` to add custom attributes:

```csharp
lambda.MapHandler(([Event] Request request) =>
{
    // Automatically traced by framework
    System.Diagnostics.Activity.Current?.SetTag("user.id", request.UserId);
    System.Diagnostics.Activity.Current?.SetTag("operation", "ProcessOrder");

    return new Response { Success = true };
});
```

### 4. Structure Logging with Tracing

Combine structured logging with tracing for complete observability:

```csharp
lambda.MapHandler(([Event] Request request, ILogger<Program> logger) =>
{
    logger.LogInformation(
        "Processing {OperationType} for user {UserId}",
        "ProcessOrder",
        request.UserId);

    // Log is automatically correlated with trace
    return new Response { Success = true };
});
```

### 5. Handle Exceptions Gracefully

Exceptions are automatically recorded in traces:

```csharp
lambda.MapHandler(([Event] Request request) =>
{
    try
    {
        return ValidateAndProcess(request);
    }
    catch (ValidationException ex)
    {
        // Exception is automatically added to span
        System.Diagnostics.Activity.Current?.SetTag("error.type", "validation");
        throw;
    }
});
```

## Troubleshooting

### Traces Not Appearing

**Check 1: OpenTelemetry is Registered**

```csharp
// Required
builder.Services.AddOpenTelemetry().WithTracing(...);
```

**Check 2: Tracing is Enabled**

```csharp
// Required
lambda.UseOpenTelemetryTracing();
```

**Check 3: Exporter is Configured**

```csharp
// Must have at least one exporter
.WithTracing(tracing => tracing
    .AddAWSLambdaConfigurations()
    .AddConsoleExporter())  // ← Need this
```

### CloudWatch Logs Not Appearing

Ensure you're using the OTLP exporter and AWS has the appropriate permissions:

```csharp
builder.Services
    .AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAWSLambdaConfigurations()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:4317");
        }));
```

Required IAM permissions:

- `logs:CreateLogGroup`
- `logs:CreateLogStream`
- `logs:PutLogEvents`

### High Memory Usage

If tracing consumes too much memory, check your sampler configuration:

```csharp
.WithTracing(tracing => tracing
    .SetDefaultTextMapPropagator(new TraceContextPropagator())
    .AddAWSLambdaConfigurations()
    .AddOtlpExporter()
    .SetSampler(new TraceIdRatioBasedSampler(0.1))  // Sample 10% of traces
);
```

## Performance Considerations

- **Compile-Time Overhead** – Generated at compile time only
- **Runtime Overhead** – Minimal; spans are created efficiently
- **Memory Impact** – ~1-2 MB for tracing infrastructure
- **Sampling** – Use `SetSampler()` to reduce trace volume in high-throughput scenarios

## Compatibility

- **.NET Versions**: .NET 9 or later (requires C# 11 interceptors)
- **Lambda Runtime**: All .NET Lambda runtimes
- **AOT Compilation**: Full support for ahead-of-time compilation
- **Frameworks**: Works with any OpenTelemetry exporter

## Related Documentation

- [OpenTelemetry Documentation](https://opentelemetry.io/docs/)
- [AWS Lambda .NET Guide](https://docs.aws.amazon.com/lambda/latest/dg/lambda-csharp.html)
- [AWS Distro for OpenTelemetry](https://aws-otel.github.io/docs/getting-started/lambda)
- [AwsLambda.Host Core Documentation](../AwsLambda.Host/README.md)

## Contributing

Contributions are welcome! Please report issues, suggest improvements, or submit pull requests on
the [GitHub repository](https://github.com/j-d-ha/dotnet-lambda-host).

## License

This project is licensed under the MIT License.

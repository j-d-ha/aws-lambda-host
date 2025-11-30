# Features

The aws-lambda-host framework provides a rich ecosystem of features and extension packages that enhance AWS Lambda development beyond the core framework capabilities.

---

## Core Framework vs Features

The framework is organized into two main categories:

### Core Framework (`AwsLambda.Host`)

The core framework provides the foundational hosting capabilities:

- **.NET Hosting Patterns** - Middleware, builder pattern, and dependency injection (similar to ASP.NET Core)
- **Async-First Design** - Native async/await support with Lambda timeout and cancellation handling
- **Source Generators & Interceptors** - Compile-time code generation for optimal performance
- **Flexible Handler Registration** - Simple, declarative API with the `[Event]` attribute
- **Lifecycle Management** - OnInit, Invocation, and OnShutdown phases

Learn more in the **[Guides](../guides/index.md)** section.

### Features & Extensions

Features extend the core framework with specialized capabilities for specific use cases:

- **Envelopes** - Type-safe event source integration packages
- **Observability** - OpenTelemetry integration for distributed tracing and metrics

---

## Feature Categories

### 1. Envelope Pattern

**What are Envelopes?**

Envelope packages wrap official AWS Lambda event classes (like `SQSEvent`, `APIGatewayProxyRequest`) and add a `BodyContent<T>` property that provides type-safe access to deserialized message payloads. Instead of manually parsing JSON strings from event bodies, you get strongly-typed objects with full IDE support and compile-time type checking.

**Key Benefits**:

- **Type Safety** - Generic type parameter `<T>` ensures compile-time type checking
- **Extensibility** - Abstract base classes allow custom serialization formats (JSON, XML, etc.)
- **Zero Overhead** - Envelopes extend official AWS event types, adding no runtime cost
- **AOT Ready** - Support for Native AOT compilation via `JsonSerializerContext` registration
- **Familiar API** - Works seamlessly with existing AWS Lambda event patterns

**Supported Event Sources**:

| Event Source                         | Package                                                                  | Use Case                                                |
|--------------------------------------|--------------------------------------------------------------------------|---------------------------------------------------------|
| **SQS**                              | [AwsLambda.Host.Envelopes.Sqs](envelopes/sqs.md)                         | Queue message processing with type-safe payloads        |
| **SNS**                              | [AwsLambda.Host.Envelopes.Sns](envelopes/sns.md)                         | Pub/sub notifications with typed messages               |
| **API Gateway**                      | [AwsLambda.Host.Envelopes.ApiGateway](envelopes/api-gateway.md)          | REST/HTTP/WebSocket APIs with request/response envelopes|
| **Kinesis Data Streams**             | [AwsLambda.Host.Envelopes.Kinesis](envelopes/kinesis.md)                 | Stream processing with typed records                    |
| **Kinesis Data Firehose**            | [AwsLambda.Host.Envelopes.KinesisFirehose](envelopes/kinesis-firehose.md)| Data transformation with typed payloads                 |
| **Kafka (MSK or self-managed)**      | [AwsLambda.Host.Envelopes.Kafka](envelopes/kafka.md)                     | Event streaming with typed messages                     |
| **CloudWatch Logs**                  | [AwsLambda.Host.Envelopes.CloudWatchLogs](envelopes/cloudwatch-logs.md)  | Log processing with typed log events                    |
| **Application Load Balancer**        | [AwsLambda.Host.Envelopes.Alb](envelopes/alb.md)                         | ALB target integration with request/response envelopes  |

!!! tip "Learn More About Envelopes"
    - **[Envelope Pattern Overview](envelopes/index.md)** - Detailed explanation of how envelopes work
    - **[Creating Custom Envelopes](envelopes/custom-envelopes.md)** - Build your own envelope implementations

### 2. Observability (OpenTelemetry)

**What is OpenTelemetry Integration?**

The `AwsLambda.Host.OpenTelemetry` package provides comprehensive observability integration for distributed tracing and metrics collection in AWS Lambda functions.

**Capabilities**:

- **Distributed Tracing** - Automatic span creation and context propagation for Lambda invocations
- **Metrics Collection** - Performance and business metrics exportable to standard observability backends
- **AWS Lambda Instrumentation** - Lambda-specific insights including cold starts, warm invocations, and error tracking
- **Lifecycle Integration** - Seamless integration with OnInit, Invocation, and OnShutdown phases
- **Vendor-Neutral** - Built on the OpenTelemetry SDK for compatibility with any observability backend

**Supported Exporters**:

- OTLP (OpenTelemetry Protocol)
- Jaeger
- AWS X-Ray
- Datadog
- New Relic
- CloudWatch
- And more...

!!! info "Learn More About OpenTelemetry"
    - **[OpenTelemetry Integration Guide](opentelemetry.md)** - Complete setup and configuration

---

## Design Philosophy

All features in the aws-lambda-host ecosystem follow these principles:

1. **Zero Runtime Overhead** - Features extend existing types rather than wrapping them
2. **AOT Compatibility** - Full support for Native AOT compilation
3. **Type Safety** - Compile-time type checking wherever possible
4. **Extensibility** - Abstract base classes and interfaces for custom implementations
5. **Familiar .NET Patterns** - Leverage existing .NET idioms and practices

---

## Quick Start

### Using Envelopes

```csharp title="Program.cs" linenums="1"
using AwsLambda.Host.Builder;
using AwsLambda.Host.Envelopes.Sqs;

var builder = LambdaApplication.CreateBuilder();
var lambda = builder.Build();

// Type-safe SQS message processing
lambda.MapHandler(
    ([Event] SqsEnvelope<OrderMessage> envelope, ILogger<Program> logger) =>
    {
        foreach (var record in envelope.Records)
        {
            if (record.BodyContent is null)
                continue;

            logger.LogInformation(
                "Processing order: {OrderId}",
                record.BodyContent.OrderId
            );
        }

        return new SQSBatchResponse();
    }
);

await lambda.RunAsync();

internal record OrderMessage(string OrderId, decimal Amount);
```

### Using OpenTelemetry

```csharp title="Program.cs" linenums="1"
using AwsLambda.Host.Builder;
using AwsLambda.Host.OpenTelemetry;

var builder = LambdaApplication.CreateBuilder();

// Add OpenTelemetry tracing and metrics
builder.Services.AddLambdaOpenTelemetry();

var lambda = builder.Build();

lambda.MapHandler(([Event] Request request) =>
{
    // Automatic span creation for this invocation
    return new Response($"Hello {request.Name}!");
});

await lambda.RunAsync();

internal record Request(string Name);
internal record Response(string Message);
```

---

## Choosing the Right Feature

### When to Use Envelopes

Use envelope packages when:

- ✅ You need type-safe access to message payloads from AWS event sources
- ✅ You want compile-time type checking for event data
- ✅ You're tired of manually parsing JSON from event bodies
- ✅ You need custom serialization formats (XML, Protobuf, etc.)
- ✅ You want IDE IntelliSense support for message structures

### When to Use OpenTelemetry

Use the OpenTelemetry package when:

- ✅ You need distributed tracing across microservices
- ✅ You want to monitor Lambda performance and cold starts
- ✅ You need to export metrics to observability platforms (Datadog, New Relic, etc.)
- ✅ You want automatic instrumentation for Lambda invocations
- ✅ You're building complex serverless architectures

---

## Installation

### Core Framework (Required)

All features require the core framework:

```bash
dotnet add package AwsLambda.Host
```

### Envelope Packages (Optional)

Install only the envelope packages you need:

```bash
# SQS envelope
dotnet add package AwsLambda.Host.Envelopes.Sqs

# API Gateway envelope
dotnet add package AwsLambda.Host.Envelopes.ApiGateway

# Other envelopes...
```

### OpenTelemetry Package (Optional)

```bash
dotnet add package AwsLambda.Host.OpenTelemetry
```

---

## Next Steps

Explore specific features:

- **[Envelope Pattern](envelopes/index.md)** - Learn how envelopes work and browse supported event sources
- **[OpenTelemetry Integration](opentelemetry.md)** - Set up distributed tracing and metrics
- **[Guides](../guides/index.md)** - Learn about core framework capabilities
- **[Examples](../examples/index.md)** - See complete examples using features

---

## Key Takeaways

1. **Features extend the core** - All features build on top of `AwsLambda.Host`
2. **Zero overhead design** - Envelopes extend official AWS types with no runtime cost
3. **Pick what you need** - Install only the envelope packages relevant to your use case
4. **AOT compatible** - All features support Native AOT compilation
5. **Type-safe by default** - Envelopes provide compile-time type checking for event payloads

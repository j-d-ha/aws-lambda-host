# Getting Started

Welcome to **aws-lambda-host**, a modern .NET framework for building AWS Lambda functions using familiar .NET patterns and best practices.

## What is aws-lambda-host?

aws-lambda-host is a .NET hosting framework that brings the familiar patterns from ASP.NET Core to AWS Lambda development. Instead of writing boilerplate code to handle Lambda events, context, and serialization, you get a clean, declarative API for defining Lambda handlers with dependency injection, middleware support, and modern async/await patterns.

Built on the generic host from Microsoft.Extensions, it provides a .NET hosting experience similar to ASP.NET Core but tailored specifically for Lambda execution.

## Why Use This Framework?

### Traditional Lambda Approach

```csharp
public class Function
{
    public string FunctionHandler(string input, ILambdaContext context)
    {
        // Manual initialization
        // Manual dependency management
        // Manual error handling
        return input.ToUpper();
    }
}
```

### aws-lambda-host Approach

```csharp
var builder = LambdaApplication.CreateBuilder();
builder.Services.AddScoped<IMyService, MyService>();

var lambda = builder.Build();
lambda.MapHandler(([Event] string input, IMyService service) =>
    service.ProcessAsync(input));

await lambda.RunAsync();
```

### Key Benefits

- **Familiar Patterns** – Use the same .NET hosting patterns you know from ASP.NET Core
- **Dependency Injection** – Built-in DI container with proper scoped lifetime management
- **Middleware Pipeline** – Compose cross-cutting concerns like logging, validation, and error handling
- **Source Generation** – Compile-time code generation eliminates reflection overhead
- **AOT Ready** – Full support for Ahead-of-Time compilation for faster cold starts
- **Type Safety** – Strongly-typed event handlers with compile-time validation

## Prerequisites

Before you begin, ensure you have:

- **.NET 8 SDK or later** – [Download here](https://dotnet.microsoft.com/download)
- **C# 11 or later** – Required for source generators and language features
- **Basic AWS Lambda knowledge** – Understanding of Lambda concepts (functions, events, execution model)
- **AWS Account** – For deploying and testing (optional for local development)

!!! tip "IDE Recommendations"
    - Visual Studio 2022 (17.8+)
    - JetBrains Rider 2023.3+
    - Visual Studio Code with C# Dev Kit

## What You'll Learn

This Getting Started guide will walk you through everything you need to build production-ready Lambda functions:

### 1. [Installation](installation.md) (~10 minutes)
- Install NuGet packages
- Configure your project file
- Verify your setup

### 2. [Your First Lambda](first-lambda.md) (~20 minutes)
- Build a complete Lambda function step-by-step
- Add dependency injection
- Test locally
- Deploy to AWS

### 3. [Core Concepts](core-concepts.md) (~30 minutes)
- Understand the Lambda lifecycle (OnInit, Invocation, OnShutdown)
- Master dependency injection patterns
- Work with middleware pipelines
- Learn about source generation

### 4. [Project Structure](project-structure.md) (~15 minutes)
- Organize your Lambda projects
- Follow best practices
- Avoid common anti-patterns
- Structure for maintainability

**Total Time**: ~85 minutes from zero to productive Lambda developer

## Framework Philosophy

aws-lambda-host is built on these core principles:

### .NET Hosting Patterns

Use the builder pattern, dependency injection, and middleware—the same patterns that make ASP.NET Core productive and maintainable.

### Async-First Design

Native support for async/await throughout the framework, with proper Lambda timeout and cancellation handling built in.

### Source Generation Benefits

Source generators analyze your handler code at compile time, generating optimized deserialization and DI resolution code. This eliminates reflection overhead and enables AOT compilation.

### Minimal Runtime Overhead

No unnecessary abstractions or reflection at runtime. The framework is designed for Lambda's constraints: fast cold starts, efficient memory usage, and predictable performance.

### Progressive Enhancement

Start simple with basic handlers, then add middleware, observability, and advanced features as your needs grow.

## Quick Example

Here's a complete Lambda function that demonstrates the framework's key features:

```csharp
using AwsLambda.Host;
using Microsoft.Extensions.DependencyInjection;

// Define your models
public record OrderRequest(string OrderId, decimal Amount);
public record OrderResponse(bool Success, string Message);

// Define your service
public interface IOrderService
{
    Task<OrderResponse> ProcessAsync(OrderRequest order);
}

public class OrderService : IOrderService
{
    public async Task<OrderResponse> ProcessAsync(OrderRequest order)
    {
        // Business logic here
        await Task.Delay(100); // Simulate processing
        return new OrderResponse(true, $"Order {order.OrderId} processed");
    }
}

// Configure and run
var builder = LambdaApplication.CreateBuilder();
builder.Services.AddScoped<IOrderService, OrderService>();

var lambda = builder.Build();

// Add middleware for logging
lambda.UseMiddleware(async (context, next) =>
{
    Console.WriteLine($"Processing request at {DateTime.UtcNow}");
    await next(context);
    Console.WriteLine($"Request completed at {DateTime.UtcNow}");
});

// Register handler with dependency injection
lambda.MapHandler(async ([Event] OrderRequest request, IOrderService service) =>
    await service.ProcessAsync(request));

await lambda.RunAsync();
```

This example shows:

- ✅ Strongly-typed request/response models using records
- ✅ Service interface and implementation
- ✅ Dependency injection registration
- ✅ Middleware for cross-cutting concerns
- ✅ Handler with automatic DI resolution
- ✅ Async/await throughout

## Next Steps

Ready to start building? Choose your path:

### For Beginners

Follow the guide in order:

1. **[Installation →](installation.md)** – Set up your environment
2. **[Your First Lambda →](first-lambda.md)** – Build a complete example
3. **[Core Concepts →](core-concepts.md)** – Understand the framework
4. **[Project Structure →](project-structure.md)** – Organize your code

### For Experienced Developers

Jump to what you need:

- **[Installation](installation.md)** – Quick setup reference
- **[Core Concepts](core-concepts.md)** – Deep dive into architecture
- **[Guides](/guides/)** – Comprehensive feature documentation
- **[Examples](/examples/)** – Complete working examples
- **[API Reference](/api-reference/)** – Detailed API docs

### Explore Features

- **[Envelopes](/features/envelopes/)** – Type-safe event source integration (SQS, SNS, API Gateway, etc.)
- **[OpenTelemetry](/features/opentelemetry.md)** – Distributed tracing and observability
- **[AOT Compilation](/advanced/aot-compilation.md)** – Optimize for fastest cold starts
- **[Source Generators](/advanced/source-generators.md)** – Understand compile-time optimizations

## Getting Help

If you run into issues or have questions:

- **[FAQ](/resources/faq.md)** – Common questions and answers
- **[Troubleshooting](/resources/troubleshooting.md)** – Solutions to common problems
- **[GitHub Issues](https://github.com/j-d-ha/aws-lambda-host/issues)** – Report bugs or request features
- **[GitHub Discussions](https://github.com/j-d-ha/aws-lambda-host/discussions)** – Ask questions and share ideas

---

Let's get started! Head to **[Installation →](installation.md)** to set up your environment.

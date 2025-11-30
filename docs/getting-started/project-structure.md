# Project Structure

As your Lambda function grows from a simple handler to a production service, proper organization becomes essential. This guide shows you how to structure Lambda projects for maintainability, testability, and scalability.

## Introduction

Good project structure helps you:

- **Find code quickly** – Organized by responsibility
- **Test effectively** – Clear separation of concerns
- **Scale smoothly** – Easy to add new features
- **Maintain easily** – Consistent patterns

We'll progress from simple to complex structures, so you can choose what fits your needs.

## Simple Lambda Structure

For straightforward Lambdas with a single responsibility:

```
MyLambda/
├── MyLambda.csproj          # Project file
├── Program.cs               # Entry point + handler
├── Models/
│   ├── Request.cs           # Input model
│   └── Response.cs          # Output model
├── Services/
│   ├── IMyService.cs        # Service interface
│   └── MyService.cs         # Service implementation
└── appsettings.json         # Configuration (optional)
```

**When to use:**
- Single event source
- Simple business logic
- 1-2 services
- Under 500 lines of code

## Modular Service Structure

For complex Lambdas with multiple services and responsibilities:

```
OrderProcessingLambda/
├── OrderProcessingLambda.csproj
├── Program.cs               # Entry point + DI registration
├── Models/
│   ├── Order.cs
│   ├── OrderItem.cs
│   ├── OrderRequest.cs
│   └── OrderResponse.cs
├── Services/
│   ├── IOrderService.cs
│   ├── OrderService.cs
│   ├── IInventoryService.cs
│   ├── InventoryService.cs
│   ├── IPaymentService.cs
│   └── PaymentService.cs
├── Repositories/
│   ├── IOrderRepository.cs
│   └── OrderRepository.cs
├── Middleware/
│   └── ValidationMiddleware.cs
├── Configuration/
│   └── OrderProcessingOptions.cs
└── appsettings.json
```

**When to use:**
- Multiple services
- Complex business logic
- Data access layers
- Reusable middleware
- Configuration management

## Program.cs Organization

Your `Program.cs` file is the entry point for your Lambda. Following a consistent organization pattern makes it easier to understand and maintain.

### Recommended Order

```csharp title="Program.cs" linenums="1"
// 1. Using statements (explicit, grouped logically)
using System;
using System.Threading;
using System.Threading.Tasks;
using AwsLambda.Host;
using Microsoft.Extensions.DependencyInjection;
using MyLambda.Services;
using MyLambda.Models;

// 2. Create builder
var builder = LambdaApplication.CreateBuilder();

// 3. Configure options (if needed)
builder.Services.Configure<OrderOptions>(
    builder.Configuration.GetSection("OrderProcessing")
);

// 4. Register services (grouped by lifetime)
// Singletons first (shared across invocations)
builder.Services.AddSingleton<ICache, MemoryCache>();
builder.Services.AddSingleton<IHttpClientFactory, HttpClientFactory>();

// Scoped services (per invocation)
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

// 5. Lifecycle hooks
builder.Services.AddOnInit(async (services, ct) =>
{
    // Initialization logic
});

builder.Services.AddOnShutdown(async (services, ct) =>
{
    // Cleanup logic
});

// 6. Build application
var lambda = builder.Build();

// 7. Register middleware (in order of execution)
lambda.UseMiddleware<LoggingMiddleware>();
lambda.UseMiddleware<ValidationMiddleware>();

// 8. Register handler
lambda.MapHandler(([Event] OrderRequest request, IOrderService service) =>
    service.ProcessAsync(request)
);

// 9. Run
await lambda.RunAsync();
```

!!! tip "Consistent Organization"
    Following this order consistently across your Lambda functions makes them easier to understand and maintain.

## Extracting Services into Files

As your `Program.cs` grows, extract services into separate files.

### Before (Inline)

```csharp title="Program.cs"
// Everything in one file
public interface IGreetingService
{
    string GetGreeting(string name);
}

public class GreetingService : IGreetingService
{
    public string GetGreeting(string name) => $"Hello, {name}!";
}

var builder = LambdaApplication.CreateBuilder();
builder.Services.AddSingleton<IGreetingService, GreetingService>();
// ...
```

**Problems:**
- Hard to test in isolation
- File becomes very long
- Difficult to reuse
- Poor separation of concerns

### After (Extracted)

```csharp title="Services/IGreetingService.cs"
namespace MyLambda.Services;

public interface IGreetingService
{
    string GetGreeting(string name);
}
```

```csharp title="Services/GreetingService.cs"
namespace MyLambda.Services;

public class GreetingService : IGreetingService
{
    public string GetGreeting(string name) => $"Hello, {name}!";
}
```

```csharp title="Program.cs"
using MyLambda.Services;

var builder = LambdaApplication.CreateBuilder();
builder.Services.AddSingleton<IGreetingService, GreetingService>();
// ...
```

**Benefits:**
- Each file has one responsibility
- Easy to find and modify
- Testable in isolation
- Reusable across projects

!!! tip "When to Extract"
    Extract to separate files when:
    - Service has more than 20 lines
    - You need to test it in isolation
    - It could be reused elsewhere
    - Program.cs exceeds 100 lines

## Test Project Structure

Organize tests to mirror your source code structure:

```
MyLambda.Tests/
├── MyLambda.Tests.csproj
├── Services/
│   ├── OrderServiceTests.cs
│   └── InventoryServiceTests.cs
├── Handlers/
│   └── OrderHandlerTests.cs
├── Middleware/
│   └── ValidationMiddlewareTests.cs
├── Fixtures/
│   ├── TestFixture.cs       # Shared test setup
│   └── TestData.cs          # Test data builders
└── Integration/
    └── LambdaIntegrationTests.cs
```

### Unit Test Example

```csharp title="Services/OrderServiceTests.cs"
using Xunit;
using NSubstitute;
using MyLambda.Services;
using MyLambda.Models;

public class OrderServiceTests
{
    [Fact]
    public async Task ProcessAsync_ValidOrder_ReturnsSuccess()
    {
        // Arrange
        var repository = Substitute.For<IOrderRepository>();
        repository.SaveAsync(Arg.Any<Order>())
            .Returns(new SaveResult { Success = true, Id = "123" });

        var service = new OrderService(repository);
        var order = new Order("123", 99.99m);

        // Act
        var result = await service.ProcessAsync(order);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("123", result.OrderId);
        await repository.Received(1).SaveAsync(order);
    }

    [Fact]
    public async Task ProcessAsync_InvalidOrder_ThrowsValidationException()
    {
        // Arrange
        var repository = Substitute.For<IOrderRepository>();
        var service = new OrderService(repository);
        var order = new Order("", -1m); // Invalid order

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.ProcessAsync(order)
        );
    }
}
```

!!! info "Testing Framework"
    This project uses xUnit with NSubstitute for mocking. Adjust patterns to match your testing framework of choice.

## Configuration Management

Manage configuration externally rather than hardcoding values.

### appsettings.json

```json title="appsettings.json"
{
  "OrderProcessing": {
    "MaxRetries": 3,
    "TimeoutSeconds": 30,
    "EnableCaching": true
  },
  "Database": {
    "ConnectionString": "Server=localhost;Database=orders",
    "CommandTimeout": 30
  },
  "ExternalApi": {
    "BaseUrl": "https://api.example.com",
    "ApiKey": "" // Set via environment variable
  }
}
```

### Options Class

Create strongly-typed configuration classes:

```csharp title="Configuration/OrderProcessingOptions.cs"
namespace MyLambda.Configuration;

public class OrderProcessingOptions
{
    public int MaxRetries { get; init; }
    public int TimeoutSeconds { get; init; }
    public bool EnableCaching { get; init; }
}
```

### Binding Configuration

Bind configuration sections to options classes:

```csharp title="Program.cs"
builder.Services.Configure<OrderProcessingOptions>(
    builder.Configuration.GetSection("OrderProcessing")
);
```

### Using Options in Services

Inject `IOptions<T>` into your services:

```csharp title="Services/OrderService.cs"
using Microsoft.Extensions.Options;

public class OrderService : IOrderService
{
    private readonly OrderProcessingOptions _options;
    private readonly IOrderRepository _repository;

    public OrderService(
        IOptions<OrderProcessingOptions> options,
        IOrderRepository repository)
    {
        _options = options.Value;
        _repository = repository;
    }

    public async Task<OrderResult> ProcessAsync(Order order)
    {
        // Use configuration
        if (_options.EnableCaching)
        {
            // Check cache...
        }

        for (int retry = 0; retry < _options.MaxRetries; retry++)
        {
            try
            {
                return await _repository.SaveAsync(order);
            }
            catch (Exception ex) when (retry < _options.MaxRetries - 1)
            {
                // Retry logic
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        throw new Exception("Max retries exceeded");
    }
}
```

## Environment-Specific Configuration

Use multiple configuration files for different environments:

### File Structure

```
MyLambda/
├── appsettings.json              # Base configuration
├── appsettings.Development.json  # Development overrides
└── appsettings.Production.json   # Production overrides
```

### Configuration Loading

```csharp title="Program.cs"
var builder = LambdaApplication.CreateBuilder();

// Configuration is automatically loaded in this order:
// 1. appsettings.json (base)
// 2. appsettings.{Environment}.json (environment-specific)
// 3. Environment variables (highest priority)

// Add additional configuration sources
builder.Configuration.AddEnvironmentVariables();
```

### Environment Variables

Access environment variables directly or through configuration:

```csharp
// Direct access
var apiKey = Environment.GetEnvironmentVariable("API_KEY");

// Through configuration (recommended)
var apiKey = builder.Configuration["ExternalApi:ApiKey"];
```

!!! warning "Never Commit Secrets"
    Never commit API keys, connection strings, or other secrets to source control. Use environment variables or AWS Secrets Manager.

## Secrets Management

### AWS Secrets Manager Integration

```csharp title="Program.cs"
using Amazon;
using Amazon.Extensions.Configuration.SystemsManager;

var builder = LambdaApplication.CreateBuilder();

// Add AWS Secrets Manager as configuration source
builder.Configuration.AddSecretsManager(
    region: RegionEndpoint.USEast1,
    configurator: options =>
    {
        options.SecretFilter = entry => entry.Name.StartsWith("MyLambda/");
        options.PollingInterval = TimeSpan.FromMinutes(5);
    }
);
```

### Environment Variables Pattern

Set sensitive values via Lambda environment variables:

```csharp
// Lambda environment variable: DATABASE_CONNECTION_STRING
var connectionString = builder.Configuration["DATABASE_CONNECTION_STRING"];

// Or through options binding
public class DatabaseOptions
{
    public string ConnectionString { get; init; } = "";
}

builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection("Database")
);
```

## Deployment Structure

### SAM Template Organization

```
MyLambda/
├── src/
│   └── MyLambda/              # Lambda source code
│       ├── Program.cs
│       └── MyLambda.csproj
├── test/
│   └── MyLambda.Tests/        # Unit tests
├── template.yaml              # SAM template
├── samconfig.toml             # SAM configuration
└── events/
    ├── order-event.json       # Test event 1
    └── error-event.json       # Test event 2
```

### CDK Project Structure

```
MyLambdaStack/
├── src/
│   ├── MyLambdaStack/         # CDK infrastructure code
│   │   ├── MyLambdaStack.cs
│   │   └── MyLambdaStack.csproj
│   └── MyLambda/              # Lambda function code
│       ├── Program.cs
│       └── MyLambda.csproj
├── test/
│   └── MyLambdaStack.Tests/
├── cdk.json
└── README.md
```

## Anti-Patterns to Avoid

### ❌ Don't: Mix Concerns in Program.cs

```csharp
// BAD: Business logic directly in Program.cs
lambda.MapHandler(([Event] Order order) =>
{
    // 50+ lines of business logic
    // Database queries
    // External API calls
    // Complex calculations
    // Validation logic
    return new OrderResponse(/* ... */);
});
```

### ✅ Do: Extract to Services

```csharp
// GOOD: Delegate to service
lambda.MapHandler(([Event] Order order, IOrderService service) =>
    service.ProcessAsync(order)
);
```

---

### ❌ Don't: Register Everything as Singleton

```csharp
// BAD: Wrong lifetime
builder.Services.AddSingleton<IOrderRepository, OrderRepository>();
// Repository with per-request state should be Scoped!
```

### ✅ Do: Use Appropriate Lifetimes

```csharp
// GOOD: Correct lifetime
builder.Services.AddSingleton<ICache, MemoryCache>();        // Stateless, shared
builder.Services.AddScoped<IOrderRepository, OrderRepository>();  // Per-request state
```

---

### ❌ Don't: Hardcode Configuration

```csharp
// BAD: Hardcoded values
public class OrderService
{
    private const int MaxRetries = 3;
    private const string ApiUrl = "https://api.example.com";
    private const int Timeout = 30;
}
```

### ✅ Do: Use Configuration Options

```csharp
// GOOD: Configuration-driven
public class OrderService
{
    private readonly OrderProcessingOptions _options;

    public OrderService(IOptions<OrderProcessingOptions> options)
    {
        _options = options.Value;
    }
}
```

---

### ❌ Don't: Put Models Everywhere

```csharp
// BAD: Models mixed with logic
public class OrderService
{
    public record OrderRequest(string Id);  // Don't define here
    public record OrderResponse(bool Success);  // Don't define here

    public OrderResponse Process(OrderRequest request) { ... }
}
```

### ✅ Do: Organize Models in Dedicated Folder

```csharp
// GOOD: Models in Models/ folder
// Models/OrderRequest.cs
namespace MyLambda.Models;
public record OrderRequest(string Id, decimal Amount);

// Models/OrderResponse.cs
namespace MyLambda.Models;
public record OrderResponse(string OrderId, bool Success);
```

---

### ❌ Don't: Use Magic Strings

```csharp
// BAD: Magic strings
var connectionString = builder.Configuration["ConnectionStrings:Default"];
var timeout = int.Parse(builder.Configuration["Timeout"]);
```

### ✅ Do: Use Strongly-Typed Options

```csharp
// GOOD: Strongly-typed configuration
public class DatabaseOptions
{
    public string ConnectionString { get; init; } = "";
    public int CommandTimeout { get; init; }
}

builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection("Database")
);
```

---

### ❌ Don't: Ignore Async/Await

```csharp
// BAD: Blocking async code
lambda.MapHandler(([Event] Order order, IOrderService service) =>
{
    var result = service.ProcessAsync(order).Result;  // DON'T!
    return result;
});
```

### ✅ Do: Use Async/Await Properly

```csharp
// GOOD: Proper async/await
lambda.MapHandler(async ([Event] Order order, IOrderService service) =>
{
    var result = await service.ProcessAsync(order);
    return result;
});
```

## Key Takeaways

1. **Start Simple**: Single file for simple Lambdas, expand as complexity grows
2. **Extract Early**: Move services to separate files before they become unwieldy
3. **Organize by Lifetime**: Group service registrations by Singleton vs Scoped
4. **Test Structure Mirrors Source**: Keep test organization consistent with source code
5. **Configuration Over Code**: Use `appsettings.json` and options pattern
6. **Secrets External**: Never commit secrets; use environment variables or AWS Secrets Manager
7. **Consistent Ordering**: Follow Program.cs organization pattern across all Lambdas
8. **Avoid Anti-Patterns**: Don't mix concerns, hardcode values, or use wrong lifetimes

## Next Steps

You now understand how to structure Lambda projects for maintainability and scalability.

### Continue Learning

- **[Middleware Patterns](/guides/middleware.md)** – Build reusable middleware components
- **[Handler Registration](/guides/handler-registration.md)** – Advanced handler patterns
- **[Testing Strategies](/guides/testing.md)** – Comprehensive testing approaches
- **[Deployment Best Practices](/guides/deployment.md)** – CI/CD and production deployments

### Explore Features

- **[Envelopes](/features/envelopes/)** – Type-safe event source integration
- **[OpenTelemetry](/features/opentelemetry.md)** – Add distributed tracing
- **[AOT Compilation](/advanced/aot-compilation.md)** – Optimize for fastest cold starts

### Browse Examples

- **[Examples](/examples/)** – Complete working examples
- **[API Reference](/api-reference/)** – Detailed API documentation

---

Congratulations! You've completed the Getting Started guide. You now have the knowledge to build, organize, and deploy production-ready Lambda functions with aws-lambda-host.

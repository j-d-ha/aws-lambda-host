# Handler Registration

Handler registration is the core of aws-lambda-host. The `MapHandler` method combined with the
`[Event]` attribute provides type-safe, reflection-free handler registration with automatic
dependency injection—all powered by compile-time source generation.

## Introduction

Unlike traditional AWS Lambda handlers that rely on reflection and method naming conventions,
aws-lambda-host uses source generators and interceptors to analyze your handler at compile time,
generating optimized code with zero runtime overhead.

**Benefits:**

- ✅ **Zero reflection** – All parameter resolution happens at compile time
- ✅ **Type-safe** – Compiler errors for missing services or incorrect signatures
- ✅ **AOT ready** – Full support for Native AOT compilation
- ✅ **Better trimming** – Only required dependencies are included
- ✅ **Faster execution** – No reflection means faster cold starts

## The MapHandler Method

### Basic Handler

The simplest handler takes an event parameter marked with `[Event]` and returns a response:

```csharp title="Program.cs"
using AwsLambda.Host;

var builder = LambdaApplication.CreateBuilder();
var lambda = builder.Build();

lambda.MapHandler(([Event] string input) => $"Hello, {input}!");

await lambda.RunAsync();
```

**How it works:**

1. Source generator analyzes the handler signature
2. Generates deserialization code for `string` input
3. Generates serialization code for `string` output
4. Creates invocation wrapper—all at compile time

### Handler with Services

Inject registered services alongside the event:

```csharp title="Program.cs"
using AwsLambda.Host;
using Microsoft.Extensions.DependencyInjection;

var builder = LambdaApplication.CreateBuilder();

builder.Services.AddScoped<IOrderService, OrderService>();

var lambda = builder.Build();

lambda.MapHandler(([Event] Order order, IOrderService service) =>
    service.Process(order)
);

await lambda.RunAsync();
```

**Source generation resolves:**

- `[Event] Order order` → Deserialized from Lambda event
- `IOrderService service` → Resolved from DI container

### Async Handlers

Use `async` handlers for I/O-bound operations:

```csharp title="Program.cs"
lambda.MapHandler(async ([Event] Order order, IOrderRepository repo) =>
{
    var result = await repo.SaveAsync(order);
    return new OrderResponse(result.Id, true);
});
```

**Always prefer `async` for:**

- Database operations
- HTTP requests
- File I/O
- Any awaitable operation

## The [Event] Attribute

The `[Event]` attribute marks the parameter that receives the deserialized Lambda event payload.

### Purpose

- **Identifies the event parameter** for source generation
- **Triggers code generation** for deserialization
- **Enables type-safe** event handling

### Rules

✅ **Required on exactly one parameter**

```csharp
// GOOD: One [Event] parameter
lambda.MapHandler(([Event] Order order, IOrderService service) => ...);
```

❌ **Cannot be omitted**

```csharp
// BAD: Missing [Event] attribute
lambda.MapHandler((Order order, IOrderService service) => ...);
// Compiler error: No event parameter marked with [Event]
```

❌ **Cannot mark multiple parameters**

```csharp
// BAD: Multiple [Event] attributes
lambda.MapHandler(([Event] Order order, [Event] string id) => ...);
// Compiler error: Only one parameter can be marked with [Event]
```

### Valid Event Types

The `[Event]` parameter can be any serializable type:

```csharp
// Primitive types
lambda.MapHandler(([Event] string input) => ...);
lambda.MapHandler(([Event] int number) => ...);

// Complex types
lambda.MapHandler(([Event] Order order) => ...);
lambda.MapHandler(([Event] OrderRequest request) => ...);

// Collections
lambda.MapHandler(([Event] List<Order> orders) => ...);
lambda.MapHandler(([Event] Dictionary<string, string> data) => ...);

// Envelopes (with envelope packages)
lambda.MapHandler(([Event] ApiGatewayRequestEnvelope<Order> request) => ...);
lambda.MapHandler(([Event] SqsEventEnvelope<Order> sqsEvent) => ...);
```

## Injectable Parameters

Handlers can inject multiple types of parameters.

### All Injectable Types

```csharp title="Program.cs"
lambda.MapHandler(async (
    [Event] Order order,                    // Lambda event (required)
    IOrderService orderService,             // Registered service
    ICache cache,                           // Another registered service
    ILambdaHostContext context,            // Framework context
    CancellationToken cancellationToken    // Timeout signal
) =>
{
    // Access invocation metadata
    var requestId = context.Items["RequestId"];

    // Use cancellation token for timeout handling
    var result = await orderService.ProcessAsync(order, cancellationToken);

    return new OrderResponse(result.Id, true);
});
```

### Parameter Resolution

| Parameter Type       | Description          | Resolved From                           |
|----------------------|----------------------|-----------------------------------------|
| `[Event] T`          | Lambda event payload | Deserialized from event JSON            |
| `IServiceType`       | Registered service   | DI container (scoped per invocation)    |
| `ILambdaHostContext` | Invocation context   | Framework (provided per invocation)     |
| `CancellationToken`  | Timeout signal       | Framework (fires before Lambda timeout) |

### Parameter Order

Parameter order doesn't matter—except `[Event]` must be present:

```csharp
// All valid - order doesn't matter
lambda.MapHandler(([Event] Order order, IOrderService service, CancellationToken ct) => ...);
lambda.MapHandler((IOrderService service, [Event] Order order, CancellationToken ct) => ...);
lambda.MapHandler((CancellationToken ct, [Event] Order order, IOrderService service) => ...);
```

### Multiple Service Injection

Inject as many services as needed:

```csharp title="Program.cs"
lambda.MapHandler(async (
    [Event] OrderRequest request,
    IOrderService orderService,
    IInventoryService inventoryService,
    IPaymentService paymentService,
    INotificationService notificationService,
    ILogger<Program> logger,
    CancellationToken ct
) =>
{
    logger.LogInformation("Processing order {OrderId}", request.OrderId);

    var inventoryOk = await inventoryService.CheckAsync(request.Items, ct);
    if (!inventoryOk) return new OrderResponse { Success = false, Reason = "Out of stock" };

    var paymentOk = await paymentService.ChargeAsync(request.Payment, ct);
    if (!paymentOk) return new OrderResponse { Success = false, Reason = "Payment failed" };

    var order = await orderService.CreateAsync(request, ct);
    await notificationService.NotifyAsync(order, ct);

    return new OrderResponse { Success = true, OrderId = order.Id };
});
```

**⚠️ Caution:** Too many dependencies may indicate the handler is doing too much. Consider
delegating to a facade service.

## Return Types

Handler return values are automatically serialized to JSON.

### Serialized Responses

```csharp title="Program.cs"
// Return simple types
lambda.MapHandler(([Event] string input) => input.ToUpper());
// Returns: "HELLO" (serialized as JSON string)

// Return complex types
lambda.MapHandler(([Event] Order order) =>
    new OrderResponse(order.Id, true)
);
// Returns: {"orderId":"123","success":true}

// Return collections
lambda.MapHandler(([Event] SearchRequest request, ISearchService search) =>
    search.Find(request.Query)
);
// Returns: [{"id":"1","name":"Item 1"}, ...]
```

### Void Returns

Handlers can return `void` or `Task` for operations with no response:

```csharp title="Program.cs"
lambda.MapHandler(([Event] LogEntry entry, ILogger<Program> logger) =>
{
    logger.LogInformation("Log entry: {Entry}", entry);
    // No return value
});

// Async void
lambda.MapHandler(async ([Event] Order order, IOrderRepository repo) =>
{
    await repo.SaveAsync(order);
    // No return value
});
```

**Response:** Empty JSON response or `null`

### Task vs ValueTask

Both `Task<T>` and `ValueTask<T>` are supported:

```csharp
// Task<T>
lambda.MapHandler(async ([Event] Order order, IOrderService service) =>
    await service.ProcessAsync(order)
);

// ValueTask<T> - for hot paths
lambda.MapHandler(async ([Event] Order order, IOrderService service) =>
{
    ValueTask<OrderResponse> result = service.ProcessAsync(order);
    return await result;
});
```

**Prefer `Task<T>`** for most cases. Use `ValueTask<T>` only for hot paths where allocation matters.

## Source Generation

Source generators analyze your handler at compile time and generate optimized code.

### How It Works

```csharp title="Program.cs"
lambda.MapHandler(([Event] Order order, IOrderService service) =>
    service.Process(order)
);
```

**Generated code** (simplified):

```csharp
// Deserialization
var order = JsonSerializer.Deserialize<Order>(eventJson);

// Service resolution
var service = context.ServiceProvider.GetRequiredService<IOrderService>();

// Invocation
var response = service.Process(order);

// Serialization
var responseJson = JsonSerializer.Serialize(response);
```

**All generated at compile time—zero runtime reflection.**

### Compile-Time Benefits

✅ **Compile-time errors** for missing services:

```csharp
lambda.MapHandler(([Event] Order order, IMissingService service) => ...);
// Compiler error if IMissingService not registered
```

✅ **Compile-time errors** for incorrect signatures:

```csharp
lambda.MapHandler((Order order) => ...);
// Compiler error: Missing [Event] attribute
```

✅ **Optimized code generation**:

```csharp
// Source generator creates optimized path for your exact signature
// No reflection, no dynamic dispatch, no runtime overhead
```

### Interceptors

The framework uses C# 12 interceptors to replace the `MapHandler` call site with generated code:

```csharp
// Your code
lambda.MapHandler(([Event] Order order) => ...);

// Intercepted and replaced with
lambda.MapHandlerInterceptor0(([Event] Order order) => ...);
// Where MapHandlerInterceptor0 is generated with optimized code
```

**Result:** Zero-cost abstraction—as if you wrote the optimized code by hand.

## Handler Patterns

### Simple CRUD Handler

```csharp title="Program.cs"
lambda.MapHandler(async (
    [Event] CreateOrderRequest request,
    IOrderRepository repo
) =>
{
    var order = new Order(request.CustomerId, request.Items, request.Total);
    await repo.CreateAsync(order);
    return new CreateOrderResponse(order.Id);
});
```

### Handler with Validation

```csharp title="Program.cs"
lambda.MapHandler(([Event] Order order, IValidator<Order> validator) =>
{
    var validationResult = validator.Validate(order);
    if (!validationResult.IsValid)
    {
        throw new ValidationException(validationResult.Errors);
    }

    return new OrderResponse(order.Id, true);
});
```

### Handler with Context Access

```csharp title="Program.cs"
lambda.MapHandler(([Event] Request request, ILambdaHostContext context) =>
{
    // Store request metadata
    context.Items["RequestId"] = request.Id;
    context.Items["Timestamp"] = DateTime.UtcNow;

    // Access shared properties
    var appVersion = context.Properties["Version"] as string;

    // Process request...
    return new Response("Success");
});
```

### Handler with Cancellation Token

```csharp title="Program.cs"
lambda.MapHandler(async (
    [Event] Order order,
    IOrderService service,
    CancellationToken ct
) =>
{
    try
    {
        return await service.ProcessAsync(order, ct);
    }
    catch (OperationCanceledException)
    {
        // Lambda timeout approaching
        return new OrderResponse { Success = false, Reason = "Timeout" };
    }
});
```

### Handler with Envelope

```csharp title="Program.cs"
using AwsLambda.Host.Envelopes.ApiGateway;

lambda.MapHandler(([Event] ApiGatewayRequestEnvelope<Order> request, ILogger<Program> logger) =>
{
    logger.LogInformation("Request from IP: {IP}", request.RequestContext.Identity.SourceIp);

    // Access payload
    var order = request.Body;

    // Process...
    return new ApiGatewayResponseEnvelope<OrderResponse>
    {
        StatusCode = 200,
        Body = new OrderResponse(order.Id, true)
    };
});
```

### Thin Handler Pattern

**Best Practice:** Keep handlers thin and delegate to services.

```csharp title="Program.cs"
// GOOD: Thin handler delegates to service
lambda.MapHandler(([Event] Order order, IOrderProcessor processor) =>
    processor.ProcessAsync(order)
);
```

```csharp title="Services/OrderProcessor.cs"
public class OrderProcessor : IOrderProcessor
{
    private readonly IOrderRepository _repository;
    private readonly IInventoryService _inventory;
    private readonly IPaymentService _payment;
    private readonly ILogger<OrderProcessor> _logger;

    public OrderProcessor(
        IOrderRepository repository,
        IInventoryService inventory,
        IPaymentService payment,
        ILogger<OrderProcessor> logger)
    {
        _repository = repository;
        _inventory = inventory;
        _payment = payment;
        _logger = logger;
    }

    public async Task<OrderResponse> ProcessAsync(Order order)
    {
        _logger.LogInformation("Processing order {OrderId}", order.Id);

        // Complex business logic here
        var inventoryOk = await _inventory.ReserveAsync(order.Items);
        if (!inventoryOk) throw new InvalidOperationException("Insufficient inventory");

        var paymentOk = await _payment.ChargeAsync(order.Payment);
        if (!paymentOk) throw new InvalidOperationException("Payment failed");

        await _repository.SaveAsync(order);

        return new OrderResponse(order.Id, true);
    }
}
```

**Why?**

- ✅ Testable (test `OrderProcessor` in isolation)
- ✅ Reusable (use `OrderProcessor` in multiple handlers)
- ✅ Maintainable (business logic separated from handler)

## Best Practices

### ✅ Do: Keep Handlers Thin

```csharp
// GOOD: Handler delegates to service
lambda.MapHandler(([Event] Order order, IOrderService service) =>
    service.ProcessAsync(order)
);
```

### ❌ Don't: Put Business Logic in Handlers

```csharp
// BAD: Business logic in handler
lambda.MapHandler(async ([Event] Order order, IOrderRepository repo, IInventoryService inventory) =>
{
    // 50+ lines of business logic
    if (order.Items.Count == 0) throw new ValidationException();
    var inventoryOk = await inventory.CheckAsync(order.Items);
    if (!inventoryOk) throw new InvalidOperationException();
    // More logic...
    return new OrderResponse(order.Id, true);
});
```

### ✅ Do: Use Async/Await for I/O

```csharp
// GOOD: Async handler for I/O operations
lambda.MapHandler(async ([Event] Order order, IOrderRepository repo) =>
    await repo.SaveAsync(order)
);
```

### ❌ Don't: Block Async Operations

```csharp
// BAD: Blocking async code
lambda.MapHandler(([Event] Order order, IOrderRepository repo) =>
    repo.SaveAsync(order).Result  // DON'T!
);
```

### ✅ Do: Inject Services, Not Factories

```csharp
// GOOD: Inject service directly
lambda.MapHandler(([Event] Order order, IOrderService service) =>
    service.ProcessAsync(order)
);
```

### ❌ Don't: Use Service Locator Pattern

```csharp
// BAD: Service locator anti-pattern
lambda.MapHandler(([Event] Order order, IServiceProvider services) =>
{
    var service = services.GetRequiredService<IOrderService>();
    return service.ProcessAsync(order);
});
```

### ✅ Do: Return Strongly-Typed Responses

```csharp
// GOOD: Strongly-typed response
lambda.MapHandler(([Event] Order order) =>
    new OrderResponse(order.Id, true)
);
```

### ❌ Don't: Return Anonymous Types

```csharp
// BAD: Anonymous type (harder to test and maintain)
lambda.MapHandler(([Event] Order order) =>
    new { orderId = order.Id, success = true }
);
```

### ✅ Do: Use CancellationToken

```csharp
// GOOD: Respect timeout signal
lambda.MapHandler(async ([Event] Order order, IOrderService service, CancellationToken ct) =>
    await service.ProcessAsync(order, ct)
);
```

## Troubleshooting

### Handler Not Found Error

**Error:**

```
System.InvalidOperationException: No handler registered
```

**Solution:**

Ensure you call `MapHandler` before `RunAsync`:

```csharp
lambda.MapHandler(([Event] string input) => ...);
await lambda.RunAsync();  // ✅
```

### Missing [Event] Attribute

**Error:**

```
Compiler error: No parameter marked with [Event] attribute
```

**Solution:**

Mark the event parameter with `[Event]`:

```csharp
lambda.MapHandler(([Event] Order order) => ...);  // ✅
```

### Service Not Registered

**Error:**

```
System.InvalidOperationException: No service for type 'IOrderService' has been registered
```

**Solution:**

Register the service before building:

```csharp
builder.Services.AddScoped<IOrderService, OrderService>();  // ✅
var lambda = builder.Build();
```

### Multiple [Event] Attributes

**Error:**

```
Compiler error: Only one parameter can be marked with [Event]
```

**Solution:**

Only mark one parameter with `[Event]`:

```csharp
lambda.MapHandler(([Event] Order order, IOrderService service) => ...);  // ✅
```

## Key Takeaways

1. **MapHandler** – Registers your Lambda handler with type-safe DI
2. **[Event] Attribute** – Marks the event parameter (required on exactly one parameter)
3. **Injectable Parameters** – `[Event] T`, registered services, `ILambdaHostContext`,
   `CancellationToken`
4. **Return Types** – Any serializable type or `void`/`Task`
5. **Source Generation** – Zero reflection, compile-time optimization, AOT ready
6. **Thin Handlers** – Delegate business logic to services
7. **Async/Await** – Always use async for I/O operations
8. **CancellationToken** – Handle Lambda timeouts gracefully

## Next Steps

Now that you understand handler registration, explore related topics:

- **[Dependency Injection](/guides/dependency-injection.md)** – Inject services into handlers
- **[Middleware](/guides/middleware.md)** – Build middleware around handlers
- **[Error Handling](/guides/error-handling.md)** – Handle exceptions in handlers
- **[Testing](/guides/testing.md)** – Test handlers in isolation
- **[Configuration](/guides/configuration.md)** – Configure handler behavior

---

Congratulations! You now understand how to register type-safe Lambda handlers with automatic
dependency injection.

# AWS Lambda Host: Architecture and Error Handling Summary

## Project Overview

**Repository**: `dotnet-lambda-host` (AWS Lambda Host for .NET)  
**Purpose**: Provides a modern host abstraction for AWS Lambda functions with dependency injection, middleware support, and OpenTelemetry instrumentation  
**Current Branch**: `features/#62-add-otel-support` - Adding conditional OpenTelemetry support  

## Project Structure

```
src/
├── AwsLambda.Host/                    # Core Lambda host library
│   ├── Application/                   # Application infrastructure
│   │   ├── DelegateHolder.cs          # Holds handler delegate, middleware list, serializers
│   │   ├── LambdaApplication.cs       # Main application facade (implements IHost, ILambdaApplication)
│   │   ├── HandlerLambdaApplicationExtensions.cs  # Extension methods for handler mapping
│   │   └── EventAttribute.cs          # Marker attribute for event parameters
│   ├── Builder/
│   │   ├── LambdaApplicationBuilder.cs      # Builder for configuring Lambda app
│   │   └── ServiceCollectionExtensions.cs   # Service registration helpers
│   ├── HostedService/
│   │   ├── LambdaHostedService.cs           # IHostedService that starts Lambda bootstrap
│   │   └── LambdaHostSettings.cs            # Configuration settings
│   ├── Context/
│   │   ├── LambdaHostContext.cs             # Context object passed to handlers
│   │   └── LambdaHostContextExtensions.cs   # Helper extensions
│   ├── Cancellation/
│   │   ├── ILambdaCancellationTokenProvider.cs  # Interface for cancellation
│   │   └── LambdaCancellationTokenSourceFactory.cs # Factory for creating CancellationTokenSource
│   ├── Middleware/
│   │   ├── DefaultMiddleware.cs             # Built-in middleware (e.g., ClearLambdaOutputFormatting)
│   │   └── MiddlewareLambdaApplicationExtensions.cs # Middleware registration
│
├── AwsLambda.Host.Abstractions/       # Public interfaces
│   ├── ILambdaApplication.cs          # Main application interface
│   ├── ILambdaHostContext.cs          # Context interface
│   └── LambdaInvocationDelegate.cs    # Handler delegate type
│
├── AwsLambda.Host.OpenTelemetry/      # OpenTelemetry integration
│   ├── LambdaOpenTelemetryExtensions.cs      # UseOpenTelemetryTracing() method
│   └── LambdaOpenTelemetryAdapters.cs        # Tracer helper methods
│
├── AwsLambda.Host.SourceGenerators/   # Roslyn source generators
│   ├── MapHandlerIncrementalGenerator.cs     # Detects MapHandler calls and generates code
│   ├── MapHandlerSourceOutput.cs             # Generates interceptor code
│   ├── MapHandlerSyntaxProvider.cs           # Syntax analysis for MapHandler detection
│   ├── Templates/
│   │   └── LambdaHandler.scriban             # Template for generating handler code
│   ├── Models/
│   │   ├── UseOpenTelemetryTracingInfo.cs    # Info about UseOpenTelemetryTracing calls
│   │   ├── CompilationInfo.cs                # Aggregated compilation data
│   │   ├── DelegateInfo.cs                   # Handler signature info
│   │   ├── MapHandlerInvocationInfo.cs       # MapHandler call info
│   │   └── ParameterInfo.cs                  # Parameter resolution info
│   └── Diagnostics.cs                       # Compiler diagnostics/warnings
```

## Current Architecture

### 1. Lambda Host Initialization Flow

```
Program.cs
  ↓
LambdaApplication.CreateBuilder()
  ↓ [creates]
LambdaApplicationBuilder (IHostApplicationBuilder wrapper)
  ↓ [configures]
  - DependencyInjection (Services)
  - Configuration (appsettings.json, env vars)
  - Logging
  - Registers LambdaHostedService as IHostedService
  - Registers DelegateHolder (singleton)
  ↓
builder.Build()
  ↓ [creates]
LambdaApplication (wraps IHost)
  ↓ [registers]
  - LambdaCancellationTokenSourceFactory
  ↓
app.MapHandler(delegate) [Generated code with InterceptsLocation]
  ↓ [sets in DelegateHolder]
  - Handler delegate
  - Optional Deserializer func
  - Optional Serializer func
  ↓
app.MapHandler(delegate) [repeat for middleware registration]
  ↓
await app.RunAsync()
  ↓ [starts]
IHost.StartAsync()
  ↓ [executes]
LambdaHostedService.StartAsync()
  ↓
  1. Builds middleware pipeline (reverse order)
  2. Wraps handler with HandlerWrapper.GetHandlerWrapper()
  3. Creates LambdaBootstrap (AWS Lambda SDK)
  4. Calls bootstrap.RunAsync() [infinite polling loop]
     - Waits for invocation events
     - Creates ILambdaHostContext
     - Calls middleware pipeline → handler
     - Returns response to Lambda runtime
```

### 2. Request/Invocation Lifecycle

```
Lambda Runtime sends invocation
  ↓
LambdaHostedService.StartAsync() wrappedHandler
  ↓
  1. Create LambdaCancellationTokenSource (with timeout buffer)
  2. Create LambdaHostContext with:
     - ILambdaContext (from Lambda runtime)
     - IServiceScopeFactory (scoped per-invocation DI)
     - CancellationToken
  3. Call Deserializer (if registered) - populates context.Event
  4. Execute middleware pipeline → handler
     - Middleware chain (built in reverse order)
     - Handler execution
  5. Call Serializer (if registered) - reads context.Response
  6. Return response Stream to Lambda runtime
  7. Dispose LambdaHostContext (cleanup scoped services)
```

### 3. Middleware Pipeline

**Design**: Follows ASP.NET Core middleware pattern  
**Pipeline Building**: 
```csharp
// In LambdaHostedService.StartAsync()
var handler = BuildMiddlewarePipeline(
    _delegateHolder.Middlewares,
    _delegateHolder.Handler!
);

// Result: middleware1 → middleware2 → handler
```

**Usage**:
```csharp
app.Use(middleware1);  // Registered in order
app.Use(middleware2);
app.MapHandler(handler);
```

**Built-in Middleware**:
- `DefaultMiddleware.ClearLambdaOutputFormatting` - Resets Console output formatting

### 4. Dependency Injection in Handlers

**Service Resolution** (compile-time via source generation):
```csharp
// User writes:
app.MapHandler((IMyService service, [Event] Request req, CancellationToken ct) => 
{
    // ...
});

// Generated code produces:
var arg0 = context.ServiceProvider.GetRequiredService<IMyService>();
var arg1 = context.GetEventT<Request>();
var arg2 = context.CancellationToken;
await castHandler.Invoke(arg0, arg1, arg2);
```

**Supported Parameter Types**:
- `[Event] T` - Event from Lambda invocation (deserialized from Stream)
- `ILambdaContext` or `ILambdaHostContext` - Context object
- `CancellationToken` - Cancellation token (will be cancelled if timeout approaches)
- `ILogger<T>` - Logger injection (resolved from DI)
- Any registered service type - Resolved from DI container
- Keyed services via `[FromKeyedServices("key")]` attribute

**Scoping**: Service scope created per-invocation in `LambdaHostContext.ServiceProvider`

---

## Error Handling Approach

### 1. Handler Execution Errors

**Error Flow**:
```
Handler throws Exception
  ↓
Exception propagates through middleware stack
  ↓
Exception bubbles up to AWS Lambda runtime
  ↓
Lambda runtime logs error and marks invocation as failed
  ↓
Lambda function container remains alive for next invocation
```

**Current Implementation** (in `LambdaHostedService.StartAsync()`):
```csharp
var wrappedHandler = HandlerWrapper.GetHandlerWrapper(
    async Task<Stream> (Stream inputStream, ILambdaContext lambdaContext) =>
    {
        using var cancellationTokenSource =
            _cancellationTokenSourceFactory.NewCancellationTokenSource(lambdaContext);

        await using var lambdaHostContext = new LambdaHostContext(
            lambdaContext,
            _scopeFactory,
            cancellationTokenSource.Token
        );

        if (_delegateHolder.Deserializer is not null)
            await _delegateHolder.Deserializer(
                lambdaHostContext,
                _settings.LambdaSerializer,
                inputStream
            );

        await handler(lambdaHostContext);

        if (_delegateHolder.Serializer is not null)
            return await _delegateHolder.Serializer(
                lambdaHostContext,
                _settings.LambdaSerializer
            );

        return new MemoryStream(0);
    }
);
```

**Exception Handling Strategy**:
- No built-in try-catch at the handler level
- Exceptions are not silently swallowed
- User can implement middleware to catch and handle exceptions
- Example: Custom error handling middleware would wrap the handler

### 2. Startup/Initialization Errors

**Bootstrap Setup Errors** (in `LambdaApplicationBuilder`):
```csharp
// Validate configuration during Build()
Services.Configure<LambdaHostSettings>(
    Configuration.GetSection(LambdaHostAppSettingsSectionName)
);

// Null checks in constructor
Services.AddHostedService<LambdaHostedService>();
Services.AddSingleton<DelegateHolder>();
```

**Handler Registration Errors** (in `LambdaHostedService` constructor):
```csharp
if (!_delegateHolder.IsHandlerSet)
    throw new InvalidOperationException("Handler is not set");
```

**Service Registration Errors**:
- Dependency injection validation happens at service resolution time
- If a required service is not registered, `GetRequiredService<T>()` throws
- Error occurs during first invocation (cold start), not at application startup

### 3. Generated Code Validation Errors

**Source Generator Diagnostics** (compile-time):
- Invalid handler signatures
- Missing service registrations (detected via semantic analysis)
- Type mismatches in parameters
- Invalid attribute usage

**Example Error** (if handler parameter can't be resolved):
```csharp
// Handler defines unsupported parameter type
app.MapHandler((UnsupportedType param) => { });

// Generator may report diagnostic at compile time
// or runtime error when trying to resolve service
```

### 4. Cancellation Token Error Handling

**Timeout Handling**:
```csharp
// LambdaCancellationTokenSourceFactory creates token with timeout buffer
// Default buffer: 3 seconds before Lambda timeout
var cancellationTokenSource =
    _cancellationTokenSourceFactory.NewCancellationTokenSource(lambdaContext);

// If Lambda timeout approaches, token is cancelled
// Handler can catch OperationCanceledException
```

**SIGTERM Handling** (from Lambda docs):
- When Lambda is shutting down, SIGTERM signal is sent
- Can be registered to cancel operations gracefully
- Currently not implemented in core, but planned in issue #44

### 5. Current Error Handling Gaps

**NOT Currently Handled**:
1. **Serialization Errors** - If deserialization of event fails, exception bubbles up
2. **Middleware Exception Handling** - No built-in error handling middleware
3. **Graceful Shutdown** - No shutdown handlers or cleanup on SIGTERM
4. **Observability** - Limited error tracking/logging (telemetry via OpenTelemetry)
5. **Startup Validation** - Limited validation of handler configuration at build time

---

## OpenTelemetry Integration (Current Implementation)

### Context: Issue #62 - Add Conditional OpenTelemetry Support

The feature branch is implementing OpenTelemetry tracing support for Lambda invocations.

### 1. Implementation Architecture

**Components**:
- `LambdaOpenTelemetryExtensions.cs` - `UseOpenTelemetryTracing()` extension method
- `LambdaOpenTelemetryAdapters.cs` - Helper methods to get tracers based on handler signature
- **Source Generator Support** - Detects `UseOpenTelemetryTracing()` calls and generates tracing middleware

### 2. Source Generator Integration

**Detection** (in `MapHandlerIncrementalGenerator.cs`):
```csharp
// Finds all calls to UseOpenTelemetryTracing()
var useOpenTelemetryTracingCalls = context
    .SyntaxProvider.CreateSyntaxProvider(
        predicate: /* detect UseOpenTelemetryTracing calls */,
        transform: /* extract location info */
    );

// Creates UseOpenTelemetryTracingInfo with location for InterceptsLocation
```

**Template Generation** (in `LambdaHandler.scriban`):
```
{{~ if is_otel_enabled ~}}
file static class LambdaHostUseOpenTelemetryTracingExtensions
{
    [InterceptsLocation(...)]
    internal static ILambdaApplication UseOpenTelemetryTracingInterceptor(...)
    {
        // Based on handler signature (input/output), select tracer variant:
        // - GetTracer<TEvent, TResponse>()     // Both input and response
        // - GetTracerNoEvent<TResponse>()       // Response only
        // - GetTracerNoResponse<TEvent>()       // Event only
        // - GetTracerNoEventNoResponse()        // Neither
    }
}
{{~ end ~}}
```

### 3. Tracer Adapter Variants

**Four variants** (in `LambdaOpenTelemetryAdapters.cs`):

1. **GetTracer<TEvent, TResponse>**
   - Validates Event type and Response type
   - Wraps handler with `AWSLambdaWrapper.TraceAsync()`
   
2. **GetTracerNoEvent<TResponse>**
   - Only validates Response type
   - No Event type validation
   
3. **GetTracerNoResponse<TEvent>**
   - Only validates Event type
   - No Response type validation
   
4. **GetTracerNoEventNoResponse**
   - No type validation needed
   - Simplest tracing wrapper

**Example** (GetTracer<TEvent, TResponse>):
```csharp
return next =>
{
    return async context =>
    {
        if (context.Event is not TEvent inputEvent)
            throw new InvalidOperationException(
                $"Lambda event of type '{typeof(TEvent).FullName}' is not available in the context."
            );

        await AWSLambdaWrapper.TraceAsync(
            tracerProvider,
            async Task<TResponse> (TEvent _, ILambdaContext _) =>
            {
                await next(context);

                if (context.Response is not TResponse result)
                    throw new InvalidOperationException(
                        $"Lambda response of type '{typeof(TResponse).FullName}' is not available in the context."
                    );

                return result;
            },
            inputEvent,
            context
        );
    };
};
```

### 4. OpenTelemetry Registration

**Expected Configuration** (from issue #62 design):
```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(configure => configure
        .AddAWSLambdaConfigurations()
        .AddConsoleExporter());

var lambda = builder.Build();
lambda.UseOpenTelemetryTracing();  // Registered via source-generated interceptor
lambda.MapHandler(handler);
```

**Critical Lambda Concerns**:
1. **Flushing Before Shutdown** - Telemetry MUST be flushed before Lambda freezes
2. **Cold Start Detection** - Track first invocation as cold start
3. **Resource Attributes** - Set AWS Lambda resource info (region, function name, version)
4. **Span Attributes** - Include AwsRequestId, FunctionName, RemainingTime, etc.

---

## Key Design Patterns

### 1. Source Generation with InterceptsLocation

**Purpose**: Replace delegate creation at compile time  
**Pattern**:
```csharp
// User writes (in Program.cs):
app.MapHandler((string input) => $"Hello, {input}!");

// Generator detects this via Roslyn and creates:
[InterceptsLocation(version, "data")]
internal static ILambdaApplication MapHandlerInterceptor(
    this ILambdaApplication application,
    Delegate handler)
{
    // Type-safe casting
    var castHandler = (Func<string, Task<string>>)handler;
    
    // Create InvocationDelegate with DI resolution
    async Task InvocationDelegate(ILambdaHostContext context)
    {
        var arg0 = context.GetEventT<string>();
        context.Response = await castHandler.Invoke(arg0);
    }
    
    return application.Map(InvocationDelegate, null, null);
}

// At runtime, the interceptor replaces the MapHandler call
```

**Benefits**:
- ✅ Type safety at compile time
- ✅ Zero reflection at runtime
- ✅ Full AOT (Ahead-of-Time) compilation support
- ✅ Excellent performance
- ✅ Rich diagnostics

### 2. Middleware Pipeline (ASP.NET Core Style)

**Design**:
```csharp
Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware =
    next => async context => 
    {
        // Pre-processing
        // Call next in chain
        await next(context);
        // Post-processing
    };
```

**Composition**:
```csharp
// Reverse order: last registered = innermost
var handler = middlewares
    .Reverse()
    .Aggregate(
        handler,
        (next, middleware) => middleware(next)
    );
```

### 3. Scoped Dependency Injection

**Per-Invocation Scopes**:
```csharp
// New scope created for each Lambda invocation
var scope = serviceScopeFactory.CreateScope();
var context = new LambdaHostContext(..., scope.ServiceProvider, ...);

// Services resolved within this scope
var service = context.ServiceProvider.GetRequiredService<IMyService>();

// Scope disposed after invocation completes
await context.DisposeAsync();  // Disposes scope
```

---

## Current State on Branch `features/#62-add-otel-support`

### Completed Work
✅ OpenTelemetry framework setup  
✅ Tracer adapter methods implemented  
✅ Source generator detects `UseOpenTelemetryTracing()` calls  
✅ Generated code creates appropriate tracer wrapper  
✅ Unit tests for OTel tracing integration  
✅ Example application with OpenTelemetry enabled  

### Pending Work (from Issue #62)
- [ ] Configuration via `LambdaHostSettings` (conditional enable/disable)
- [ ] OTLP exporter support
- [ ] AWS X-Ray exporter support
- [ ] Graceful telemetry flushing before Lambda shutdown
- [ ] Cold start detection
- [ ] Resource attribute configuration
- [ ] Metrics support (not just tracing)
- [ ] Documentation and examples

---

## Related Issues for Error Handling

### Issue #52: Startup Handler Callbacks
- Register `OnStartup` callbacks before first invocation
- Similar source generation pattern as `MapHandler`
- **Benefit for Error Handling**: Initialize resources safely before handling requests

### Issue #46/#44: SIGTERM Cancellation Support
- Lambda sends SIGTERM when shutting down container
- Need to gracefully cleanup and flush telemetry
- **Benefit for Error Handling**: Proper resource cleanup before process termination

### Issue #53: Shutdown Handler Callbacks
- Register `OnShutdown` callbacks before Lambda container termination
- **Benefit for Error Handling**: Final cleanup, telemetry flushing, graceful degradation

---

## Summary: Error Handling Approach

The Lambda Host currently uses a **"fail-open"** error handling strategy:

1. **Validation Errors** occur at:
   - Compile-time (via source generator diagnostics)
   - Application startup (missing services)
   - First invocation (unresolvable dependencies)

2. **Handler Errors** propagate directly to Lambda runtime:
   - No built-in error recovery
   - User can implement middleware for error handling
   - Exceptions are logged by Lambda platform

3. **Key Design Philosophy**:
   - **Minimal overhead** - No catching/logging at framework level
   - **Transparency** - Errors bubble up naturally
   - **Extensibility** - Middleware enables custom error handling
   - **Type Safety** - Source generation prevents many errors at compile-time

4. **Gaps Being Addressed**:
   - OpenTelemetry integration for observability
   - Startup/shutdown handlers for resource initialization
   - SIGTERM support for graceful shutdown
   - Better async serialization support


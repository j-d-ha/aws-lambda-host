# Task: Implement Composition/Delegation Pattern for LambdaApplicationBuilder

## Objective
Implement `LambdaApplicationBuilder` as a passthrough for `HostApplicationBuilder` using composition/delegation pattern. The builder will compose an internal `HostApplicationBuilder` and delegate all operations to it, while returning a `LambdaApplication` that wraps the built `IHost`.

## Steps

### 1. Update LambdaApplicationBuilder Implementation
   - 1.1 Add private field to hold internal `HostApplicationBuilder` instance
   - 1.2 Add constructors that create internal `HostApplicationBuilder` with proper arguments
   - 1.3 Implement property delegation to internal builder for all `IHostApplicationBuilder` properties:
     - `Configuration` → delegate to internal builder
     - `Environment` → delegate to internal builder  
     - `Logging` → delegate to internal builder
     - `Metrics` → delegate to internal builder
     - `Services` → delegate to internal builder
     - `Properties` → delegate to internal builder
   - 1.4 Implement `ConfigureContainer` method delegation
   - 1.5 Add `Build()` method that:
     - Calls internal builder's `Build()` method
     - Creates and returns `LambdaApplication` wrapping the built host

### 2. Update LambdaApplication Implementation  
   - 2.1 Add private field to hold composed `IHost` instance
   - 2.2 Add constructor that accepts `IHost` parameter
   - 2.3 Implement method delegation for all `IHost` members:
     - `StartAsync` → delegate to internal host
     - `StopAsync` → delegate to internal host
     - `Dispose` → delegate to internal host
     - `DisposeAsync` → delegate to internal host
   - 2.4 Implement property delegation:
     - `Services` → delegate to internal host

### 3. Add Factory Methods (Optional Enhancement)
   - 3.1 Add static `CreateBuilder()` method to `LambdaApplicationBuilder`
   - 3.2 Add static `CreateBuilder(string[] args)` overload
   - 3.3 Add static `CreateBuilder(HostApplicationBuilderSettings)` overload

### 4. Update Example Usage
   - 4.1 Modify example in HelloWorld to demonstrate the new builder pattern
   - 4.2 Show integration with existing .NET hosting patterns

## Technical Considerations

- **Lifecycle Management**: Ensure proper disposal chain from `LambdaApplication` to internal `IHost`
- **Thread Safety**: Delegate thread safety concerns to underlying `HostApplicationBuilder`
- **Build State**: Handle the fact that `HostApplicationBuilder.Build()` can only be called once
- **Error Handling**: Preserve original exception behavior and stack traces

## Deliverables

- Updated `LambdaApplicationBuilder.cs` with full composition/delegation implementation
- Updated `LambdaApplication.cs` with proper host wrapping
- Optional: Factory methods for easier builder creation
- Optional: Updated example showing usage pattern similar to standard .NET hosting
- All existing functionality preserved while adding .NET hosting integration
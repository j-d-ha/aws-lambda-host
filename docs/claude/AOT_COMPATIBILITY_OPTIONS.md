# AOT Compatibility Options for Lambda.Host

**Document Version:** 1.0
**Date:** 2025-10-07
**Purpose:** Analysis and recommendations for replacing assembly scanning with AOT-compatible alternatives

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Current Architecture Analysis](#current-architecture-analysis)
3. [The AOT Problem](#the-aot-problem)
4. [Solution Options](#solution-options)
   - [Option 1: Extension Method Registration Pattern](#option-1-extension-method-registration-pattern)
   - [Option 2: Partial Class Registration Pattern](#option-2-partial-class-registration-pattern)
   - [Option 3: Module Initializer Pattern](#option-3-module-initializer-pattern)
   - [Option 4: Attribute-Based Registry Pattern](#option-4-attribute-based-registry-pattern)
   - [Option 5: Hybrid Approach with Feature Flags](#option-5-hybrid-approach-with-feature-flags)
5. [Comparison Matrix](#comparison-matrix)
6. [Recommendations](#recommendations)
7. [Implementation Roadmap](#implementation-roadmap)

---

## Executive Summary

### The Problem

The `LambdaApplicationBuilder.Build()` method currently uses reflection-based assembly scanning to discover and register `IHostedService` implementations:

```csharp
var hostedServiceTypes = Assembly
    .GetCallingAssembly()
    .GetTypes()
    .Where(t => t.IsClass && !t.IsAbstract && typeof(IHostedService).IsAssignableFrom(t))
    .ToArray();
```

**This pattern is incompatible with .NET Native AOT** because:
- `Assembly.GetCallingAssembly()` requires runtime reflection
- `GetTypes()` requires runtime type enumeration
- `IsAssignableFrom()` requires runtime type checking

### The Solution

Replace reflection-based discovery with **compile-time code generation** using source generators. The Lambda.Host project already has sophisticated source generators in place for creating `LambdaStartupService` classes, making this transition straightforward.

### Recommended Approach

**Option 1: Extension Method Registration Pattern** (recommended for most scenarios)
- Simple, explicit, and follows .NET conventions
- Minimal breaking changes
- Clear migration path
- Industry-standard approach (used by ASP.NET Core, EF Core)

**Quick Win:** For immediate AOT support with minimal changes, implement Option 1. For zero breaking changes, consider Option 5 (Hybrid Approach).

---

## Current Architecture Analysis

### How It Works Today

1. **User writes code:**
   ```csharp
   var builder = LambdaApplication.CreateBuilder();
   var lambda = builder.Build();
   lambda.MapHandler(([Request] string input) => "hello world");
   await lambda.RunAsync();
   ```

2. **Source generator creates `LambdaStartupService`:**
   ```csharp
   // Auto-generated in obj/Generated/
   namespace Lambda.Host.Example.HelloWorld;

   public class LambdaStartupService : IHostedService
   {
       // ... implementation
   }
   ```

3. **Build() uses reflection to find services:**
   ```csharp
   public LambdaApplication Build()
   {
       var hostedServiceTypes = Assembly
           .GetCallingAssembly()
           .GetTypes()
           .Where(t => t.IsClass && !t.IsAbstract && typeof(IHostedService).IsAssignableFrom(t))
           .ToArray();

       foreach (var serviceType in hostedServiceTypes)
       {
           Services.AddSingleton(serviceType);
           Services.AddSingleton<IHostedService>(sp =>
               (IHostedService)sp.GetRequiredService(serviceType));
       }

       // ... rest of Build logic
   }
   ```

### Files Involved

| File | Purpose | AOT Compatible? |
|------|---------|----------------|
| `LambdaApplicationBuilder.cs:44-62` | Scans for IHostedService implementations | ❌ No |
| `MapHandlerIncrementalGenerator.cs` | Creates LambdaStartupService via source generation | ✅ Yes |
| `MapHandlerSourceOutput.cs` | Generates the service code | ✅ Yes |
| `Templates/LambdaStartupService.scriban` | Template for generated service | ✅ Yes |

**Key Insight:** Only the registration mechanism in `LambdaApplicationBuilder.Build()` needs to change. The source generation pipeline is already AOT-compatible.

---

## The AOT Problem

### .NET Native AOT Restrictions

From Microsoft documentation, Native AOT has several critical limitations:

1. **No dynamic loading**: `Assembly.LoadFile`, `Assembly.GetCallingAssembly()`
2. **No runtime code generation**: `System.Reflection.Emit`
3. **Limited reflection**: Type graph walking is not supported
4. **Trimming required**: All unused code is removed at compile time

### Why Assembly Scanning Fails

```csharp
Assembly.GetCallingAssembly()  // ❌ Requires runtime assembly information
    .GetTypes()                // ❌ Requires runtime type enumeration
    .Where(t => typeof(IHostedService).IsAssignableFrom(t))  // ❌ Runtime type checking
```

When compiled with AOT:
- The compiler cannot determine which types exist at runtime
- Reflection metadata is trimmed away
- Type relationships are not preserved

### AOT Warning Example

```
warning IL3050: Using member 'System.Reflection.Assembly.GetTypes()' which has
'RequiresUnreferencedCodeAttribute' can break functionality when trimming
application code. Types might be removed.
```

---

## Solution Options

All options below maintain the existing user-facing API (`MapHandler`) while replacing the reflection-based registration.

---

## Option 1: Extension Method Registration Pattern

### Overview

The source generator creates an extension method on `IServiceCollection` that explicitly registers the generated `LambdaStartupService`. Users call this method in their startup code.

### How It Works

**Generated Code:**
```csharp
// Auto-generated by Lambda.Host.SourceGenerators
namespace Lambda.Host.Example.HelloWorld;

public static class LambdaHostServiceExtensions
{
    public static IServiceCollection AddLambdaHostServices(
        this IServiceCollection services)
    {
        services.AddSingleton<LambdaStartupService>();
        services.AddSingleton<IHostedService>(sp =>
            sp.GetRequiredService<LambdaStartupService>());

        return services;
    }
}
```

**User Code Changes:**
```csharp
var builder = LambdaApplication.CreateBuilder();

// NEW: Add this line
builder.Services.AddLambdaHostServices();

var lambda = builder.Build();
lambda.MapHandler(([Request] string input) => "hello world");
await lambda.RunAsync();
```

**LambdaApplicationBuilder Changes:**
```csharp
public LambdaApplication Build()
{
    // Remove assembly scanning entirely
    // ❌ DELETE: var hostedServiceTypes = Assembly.GetCallingAssembly().GetTypes()...

    Services.AddSingleton<DelegateHolder>();
    Services.TryAddSingleton<ILambdaCancellationTokenSourceFactory>(...);
    Services.TryAddSingleton<ILambdaSerializer>(...);

    var host = _hostBuilder.Build();
    return new LambdaApplication(host);
}
```

### Pros

✅ **Simple and explicit** - Clear what's being registered
✅ **Standard .NET pattern** - Used by EF Core (`AddDbContext`), ASP.NET Core, etc.
✅ **Easy to test** - Can verify registration in unit tests
✅ **No magic** - Developers understand the flow
✅ **Tooling support** - IntelliSense shows the extension method
✅ **Minimal generator changes** - Straightforward to implement
✅ **AOT compatible** - Pure compile-time code generation

### Cons

❌ **Breaking change** - Requires user code modification
❌ **One more line** - Users must remember to call `AddLambdaHostServices()`
❌ **Potential confusion** - "Why do I need to call this if services are auto-generated?"

### Implementation Strategy

#### Phase 1: Update Source Generator

**File:** `src/Lambda.Host.SourceGenerators/MapHandlerSourceOutput.cs`

Add a new template rendering step after generating `LambdaStartup.g.cs`:

```csharp
internal static void Generate(
    SourceProductionContext context,
    (ImmutableArray<MapHandlerInvocationInfo> delegateInfos, bool compilationHasErrors) combined)
{
    // ... existing code that generates LambdaStartup.g.cs ...

    context.AddSource("LambdaStartup.g.cs", outCode);

    // NEW: Generate extension method
    var extensionModel = new
    {
        Namespace = delegateInfo.Namespace,
        ServiceClassName = "LambdaStartupService"
    };

    var extensionTemplate = TemplateHelper.LoadTemplate(
        GeneratorConstants.LambdaHostServiceExtensionsTemplateFile
    );

    var extensionCode = extensionTemplate.Render(extensionModel);
    context.AddSource("LambdaHostServiceExtensions.g.cs", extensionCode);
}
```

#### Phase 2: Create Extension Method Template

**File:** `src/Lambda.Host.SourceGenerators/Templates/LambdaHostServiceExtensions.scriban`

```csharp
// <auto-generated>
//     Generated by the Lambda.Host source generator.
// </auto-generated>

#nullable enable

namespace {{ namespace }};

/// <summary>
/// Extension methods for registering Lambda.Host services.
/// </summary>
public static class LambdaHostServiceExtensions
{
    /// <summary>
    /// Adds Lambda.Host services to the specified <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The same service instance for chaining.</returns>
    public static global::Microsoft.Extensions.DependencyInjection.IServiceCollection AddLambdaHostServices(
        this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)
    {
        services.AddSingleton<{{ service_class_name }}>();
        services.AddSingleton<global::Microsoft.Extensions.Hosting.IHostedService>(
            serviceProvider => serviceProvider.GetRequiredService<{{ service_class_name }}>()
        );

        return services;
    }
}
```

#### Phase 3: Update LambdaApplicationBuilder

**File:** `src/Lambda.Host/LambdaApplicationBuilder.cs`

```csharp
public LambdaApplication Build()
{
    // REMOVE assembly scanning code (lines 44-62)

    Services.AddSingleton<DelegateHolder>();
    Services.TryAddSingleton<ILambdaCancellationTokenSourceFactory>(
        _ => new LambdaCancellationTokenSourceFactory(_defaultCancellationBuffer)
    );
    Services.TryAddSingleton<ILambdaSerializer>(_ => new DefaultLambdaJsonSerializer());

    var host = _hostBuilder.Build();
    return new LambdaApplication(host);
}
```

#### Phase 4: Add Diagnostic Warning (Optional)

Add a build warning if users forget to call `AddLambdaHostServices()`:

```csharp
// In MapHandlerSourceOutput.cs
private static void ValidateServiceRegistration(...)
{
    // Check if AddLambdaHostServices is called anywhere in the compilation
    // If not, emit a warning diagnostic
}
```

#### Phase 5: Update Documentation

Create migration guide in `MIGRATION.md`:

```markdown
## Migrating to AOT-Compatible Lambda.Host

### Required Changes

Add the following line after creating your builder:

```csharp
builder.Services.AddLambdaHostServices();
```

### Before
```csharp
var builder = LambdaApplication.CreateBuilder();
var lambda = builder.Build();
```

### After
```csharp
var builder = LambdaApplication.CreateBuilder();
builder.Services.AddLambdaHostServices();  // Add this line
var lambda = builder.Build();
```
```

### Testing Strategy

```csharp
[Fact]
public void Extension_Method_Should_Register_Services()
{
    // Arrange
    var services = new ServiceCollection();

    // Act
    services.AddLambdaHostServices();
    var serviceProvider = services.BuildServiceProvider();

    // Assert
    var hostedServices = serviceProvider.GetServices<IHostedService>();
    Assert.Single(hostedServices);
    Assert.IsType<LambdaStartupService>(hostedServices.First());
}
```

---

## Option 2: Partial Class Registration Pattern

### Overview

Make `LambdaApplicationBuilder` a partial class and generate the registration code in a partial method. This keeps registration automatic but AOT-compatible.

### How It Works

**LambdaApplicationBuilder.cs (modified):**
```csharp
public sealed partial class LambdaApplicationBuilder : IHostApplicationBuilder
{
    public LambdaApplication Build()
    {
        // Call generated partial method
        RegisterGeneratedServices();

        Services.AddSingleton<DelegateHolder>();
        // ... rest of Build logic
    }

    // Partial method to be implemented by source generator
    partial void RegisterGeneratedServices();
}
```

**Generated Partial Class:**
```csharp
// Auto-generated
namespace Lambda.Host;

public sealed partial class LambdaApplicationBuilder
{
    partial void RegisterGeneratedServices()
    {
        Services.AddSingleton<Lambda.Host.Example.HelloWorld.LambdaStartupService>();
        Services.AddSingleton<IHostedService>(sp =>
            sp.GetRequiredService<Lambda.Host.Example.HelloWorld.LambdaStartupService>());
    }
}
```

### Pros

✅ **Zero user code changes** - Works with existing code
✅ **Automatic registration** - No explicit calls needed
✅ **AOT compatible** - Pure compile-time generation
✅ **Seamless transition** - Drop-in replacement

### Cons

❌ **Namespace complexity** - Generated service is in user's namespace, builder is in `Lambda.Host` namespace
❌ **Cross-assembly generation** - Generator must inject code into Lambda.Host assembly space
❌ **Less explicit** - "Magic" behavior might confuse developers
❌ **Complex implementation** - Harder to get right
❌ **Testing challenges** - Partial methods harder to test in isolation

### Implementation Strategy

#### Challenge: Cross-Assembly Partial Methods

**The Problem:** The generated `LambdaStartupService` is in the user's project namespace (e.g., `Lambda.Host.Example.HelloWorld`), but `LambdaApplicationBuilder` is in the `Lambda.Host` namespace. Partial methods must be in the same namespace.

**Solutions:**

**Approach A: Generate Registration in User Namespace**
```csharp
// In user's namespace
namespace Lambda.Host.Example.HelloWorld;

internal static class LambdaHostRegistration
{
    internal static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<LambdaStartupService>();
        services.AddSingleton<IHostedService>(sp =>
            sp.GetRequiredService<LambdaStartupService>());
    }
}
```

Then have `LambdaApplicationBuilder` call this via reflection... wait, that defeats the purpose! ❌

**Approach B: Use Global Namespace Tricks**
```csharp
// Generated in global namespace
namespace Lambda.Host;

file static class LambdaHostRegistrationHelper
{
    internal static void Register(IServiceCollection services)
    {
        // Fully qualified type name
        services.AddSingleton<global::Lambda.Host.Example.HelloWorld.LambdaStartupService>();
        // ...
    }
}

// Existing file
public partial class LambdaApplicationBuilder
{
    partial void RegisterGeneratedServices()
    {
        LambdaHostRegistrationHelper.Register(Services);
    }
}
```

**Verdict:** This approach is complex and fragile. **Not recommended** unless zero breaking changes is an absolute requirement.

---

## Option 3: Module Initializer Pattern

### Overview

Use the `[ModuleInitializer]` attribute (C# 9.0+) to register services automatically when the module loads. This provides "magic" registration without user code changes.

### How It Works

**Generated Code:**
```csharp
namespace Lambda.Host.Example.HelloWorld;

internal static class LambdaHostModuleInitializer
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void Initialize()
    {
        LambdaHostServiceRegistry.Register(typeof(LambdaStartupService));
    }
}
```

**Registry Class (in Lambda.Host):**
```csharp
namespace Lambda.Host;

public static class LambdaHostServiceRegistry
{
    private static readonly List<Type> RegisteredServiceTypes = new();

    public static void Register(Type serviceType)
    {
        RegisteredServiceTypes.Add(serviceType);
    }

    internal static IEnumerable<Type> GetRegisteredTypes() => RegisteredServiceTypes;
}
```

**LambdaApplicationBuilder:**
```csharp
public LambdaApplication Build()
{
    foreach (var serviceType in LambdaHostServiceRegistry.GetRegisteredTypes())
    {
        Services.AddSingleton(serviceType);
        Services.AddSingleton<IHostedService>(sp =>
            (IHostedService)sp.GetRequiredService(serviceType));
    }

    // ... rest of Build logic
}
```

### Pros

✅ **Zero user code changes** - Completely automatic
✅ **Modern .NET feature** - Uses C# 9.0 module initializers
✅ **Clear separation** - Registration logic separate from user code

### Cons

❌ **Still uses Type** - Registry stores `Type` instances
❌ **Not fully AOT-safe** - `Type` storage and `sp.GetRequiredService(serviceType)` requires reflection
❌ **Static state** - Global registry adds complexity
❌ **Initialization order** - Module initializers run in undefined order
❌ **Hard to debug** - Magic initialization is hard to trace

### Verdict

**Not recommended** - While module initializers are useful, this approach still relies on runtime type information and `GetRequiredService(Type)`, which may trigger AOT warnings.

---

## Option 4: Attribute-Based Registry Pattern

### Overview

Similar to ASP.NET Core's Request Delegate Generator (RDG), use attributes to mark generated services and create a compile-time registry.

### How It Works

**Attribute Definition (in Lambda.Host):**
```csharp
namespace Lambda.Host;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class LambdaHostServiceAttribute : Attribute
{
}
```

**Generated Service (marked with attribute):**
```csharp
namespace Lambda.Host.Example.HelloWorld;

[LambdaHostService]
public class LambdaStartupService : IHostedService
{
    // ... implementation
}
```

**Generated Registry:**
```csharp
namespace Lambda.Host.Example.HelloWorld;

public static class LambdaHostServiceRegistry
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<LambdaStartupService>();
        services.AddSingleton<IHostedService>(sp =>
            sp.GetRequiredService<LambdaStartupService>());
    }
}
```

**User calls registry:**
```csharp
builder.Services.RegisterServices();  // Generated extension method
```

### Pros

✅ **Discoverable via attributes** - Can find services via metadata
✅ **Compile-time registration** - Registry is generated
✅ **AOT compatible** - No runtime reflection
✅ **Extensible** - Can add metadata to attributes

### Cons

❌ **Similar to Option 1** - User still needs to call registration
❌ **Extra indirection** - Attribute adds no value over direct generation
❌ **More generated code** - Attribute + Registry + Extension method

### Verdict

**Not recommended** - This is essentially Option 1 with extra steps. The attribute provides no meaningful benefit over directly generating the extension method.

---

## Option 5: Hybrid Approach with Feature Flags

### Overview

Support **both** reflection-based (default) and source generation-based (opt-in) registration. This provides a gradual migration path and maintains backward compatibility.

### How It Works

**Feature Flag (in user's .csproj):**
```xml
<PropertyGroup>
    <LambdaHostUseSourceGeneration>true</LambdaHostUseSourceGeneration>
</PropertyGroup>
```

**Generated Code (when flag is enabled):**
```csharp
namespace Lambda.Host.Example.HelloWorld;

public static class LambdaHostServiceExtensions
{
    public static IServiceCollection AddLambdaHostServices(
        this IServiceCollection services)
    {
        services.AddSingleton<LambdaStartupService>();
        services.AddSingleton<IHostedService>(sp =>
            sp.GetRequiredService<LambdaStartupService>());

        return services;
    }
}
```

**LambdaApplicationBuilder (with conditional logic):**
```csharp
public LambdaApplication Build()
{
#if !LAMBDA_HOST_USE_SOURCE_GENERATION
    // Traditional reflection-based discovery (default)
    var hostedServiceTypes = Assembly
        .GetCallingAssembly()
        .GetTypes()
        .Where(t => t.IsClass && !t.IsAbstract && typeof(IHostedService).IsAssignableFrom(t))
        .ToArray();

    foreach (var serviceType in hostedServiceTypes)
    {
        Services.AddSingleton(serviceType);
        Services.AddSingleton<IHostedService>(sp =>
            (IHostedService)sp.GetRequiredService(serviceType));
    }
#endif

    Services.AddSingleton<DelegateHolder>();
    // ... rest of Build logic
}
```

**User code when opting in:**
```csharp
// .csproj has <LambdaHostUseSourceGeneration>true</LambdaHostUseSourceGeneration>

var builder = LambdaApplication.CreateBuilder();
#if LAMBDA_HOST_USE_SOURCE_GENERATION
builder.Services.AddLambdaHostServices();
#endif
var lambda = builder.Build();
```

### Pros

✅ **Zero breaking changes** - Existing code continues to work
✅ **Gradual migration** - Users can opt-in when ready
✅ **Easy to test** - Can compare both paths
✅ **Risk mitigation** - Fallback if source generation has issues
✅ **Clear deprecation path** - Can remove reflection in v2.0

### Cons

❌ **Maintenance burden** - Two code paths to maintain
❌ **Complexity** - More configuration options
❌ **User confusion** - "Which mode am I using?"
❌ **Delayed transition** - Users may never migrate

### Implementation Strategy

#### Phase 1: Add Feature Flag Support

**File:** `src/Lambda.Host.SourceGenerators/Lambda.Host.SourceGenerators.csproj`

```xml
<PropertyGroup>
    <!-- Emit build property to user's compilation -->
    <EmitCompilerVisibleProperty>true</EmitCompilerVisibleProperty>
</PropertyGroup>
```

**File:** User's `.csproj`

```xml
<PropertyGroup>
    <LambdaHostUseSourceGeneration>true</LambdaHostUseSourceGeneration>
</PropertyGroup>
```

#### Phase 2: Update Source Generator

**File:** `src/Lambda.Host.SourceGenerators/MapHandlerIncrementalGenerator.cs`

```csharp
public void Initialize(IncrementalGeneratorInitializationContext context)
{
    // Check if source generation mode is enabled
    var useSourceGeneration = context.AnalyzerConfigOptionsProvider
        .Select((provider, _) =>
        {
            provider.GlobalOptions.TryGetValue(
                "build_property.LambdaHostUseSourceGeneration",
                out var value);
            return bool.TryParse(value, out var result) && result;
        });

    // Only generate extension methods if flag is enabled
    var combined = mapHandlerCalls
        .Collect()
        .Combine(compilationHasErrors)
        .Combine(useSourceGeneration);

    context.RegisterSourceOutput(combined, MapHandlerSourceOutput.Generate);
}
```

#### Phase 3: Conditional Compilation in LambdaApplicationBuilder

**File:** `src/Lambda.Host/LambdaApplicationBuilder.cs`

```csharp
public LambdaApplication Build()
{
#if !LAMBDA_HOST_USE_SOURCE_GENERATION
    var hostedServiceTypes = Assembly
        .GetCallingAssembly()
        .GetTypes()
        .Where(t => t.IsClass && !t.IsAbstract && typeof(IHostedService).IsAssignableFrom(t))
        .ToArray();

    foreach (var serviceType in hostedServiceTypes)
    {
        Services.AddSingleton(serviceType);
        Services.AddSingleton<IHostedService>(serviceProvider =>
            (IHostedService)serviceProvider.GetRequiredService(serviceType)
        );
    }
#endif

    Services.AddSingleton<DelegateHolder>();
    Services.TryAddSingleton<ILambdaCancellationTokenSourceFactory>(
        _ => new LambdaCancellationTokenSourceFactory(_defaultCancellationBuffer)
    );
    Services.TryAddSingleton<ILambdaSerializer>(_ => new DefaultLambdaJsonSerializer());

    var host = _hostBuilder.Build();
    return new LambdaApplication(host);
}
```

#### Phase 4: Documentation

**File:** `README.md`

```markdown
## AOT Compatibility

Lambda.Host supports .NET Native AOT compilation. To enable AOT-compatible mode:

1. Add the feature flag to your `.csproj`:
   ```xml
   <PropertyGroup>
       <LambdaHostUseSourceGeneration>true</LambdaHostUseSourceGeneration>
       <PublishAot>true</PublishAot>
   </PropertyGroup>
   ```

2. Call the generated registration method:
   ```csharp
   var builder = LambdaApplication.CreateBuilder();
   builder.Services.AddLambdaHostServices();
   var lambda = builder.Build();
   ```
```

### Migration Timeline

```
Version 1.x (Current)
├─ Default: Reflection-based discovery
└─ Opt-in: Source generation via feature flag

Version 2.0 (Future)
├─ Default: Source generation
├─ Deprecated: Reflection-based discovery (with warning)
└─ Breaking: Must call AddLambdaHostServices()

Version 3.0 (Future)
└─ Removed: Reflection-based discovery
```

---

## Comparison Matrix

| Feature | Option 1<br/>Extension Method | Option 2<br/>Partial Class | Option 3<br/>Module Initializer | Option 4<br/>Attribute Registry | Option 5<br/>Hybrid |
|---------|-------------------------------|----------------------------|----------------------------------|----------------------------------|---------------------|
| **AOT Compatible** | ✅ Yes | ✅ Yes | ⚠️ Partial | ✅ Yes | ✅ Yes (when enabled) |
| **Breaking Changes** | ❌ Yes | ✅ No | ✅ No | ❌ Yes | ✅ No |
| **User Code Changes** | Required | None | None | Required | Optional |
| **Implementation Complexity** | 🟢 Low | 🔴 High | 🟡 Medium | 🟡 Medium | 🔴 High |
| **Testability** | 🟢 Easy | 🟡 Medium | 🔴 Hard | 🟢 Easy | 🟡 Medium |
| **Debuggability** | 🟢 Clear | 🟡 Medium | 🔴 Opaque | 🟢 Clear | 🟡 Medium |
| **Maintainability** | 🟢 High | 🟡 Medium | 🟡 Medium | 🟡 Medium | 🔴 Low (2 paths) |
| **Industry Standard** | ✅ Yes | ❌ Rare | ❌ Uncommon | ⚠️ Specialized | ⚠️ Transitional |
| **Documentation Burden** | 🟢 Low | 🟢 Low | 🟡 Medium | 🟡 Medium | 🔴 High |
| **Migration Path** | Direct | N/A | N/A | Direct | Gradual |
| **Recommended For** | New projects, major versions | Legacy support | ❌ Not recommended | ❌ Not recommended | Conservative upgrades |

### Legend
- 🟢 Good / Low effort
- 🟡 Moderate / Medium effort
- 🔴 Poor / High effort

---

## Recommendations

### Scenario-Based Recommendations

#### Scenario 1: New Project (Greenfield)
**Recommendation:** **Option 1 - Extension Method Pattern**

**Rationale:**
- Clean, modern approach
- No legacy constraints
- Easy to understand and maintain
- Follows .NET ecosystem conventions

**Action Plan:**
1. Implement Option 1
2. Update example projects
3. Document in README

---

#### Scenario 2: Existing Project with Active Users (Minimal Disruption)
**Recommendation:** **Option 5 - Hybrid Approach**

**Rationale:**
- Zero breaking changes initially
- Users can migrate at their own pace
- Provides clear upgrade path
- Can deprecate reflection in next major version

**Action Plan:**
1. Implement Option 5 for v1.x
2. Release with documentation on opting in
3. Deprecate reflection in v2.0
4. Remove reflection in v3.0

---

#### Scenario 3: Existing Project with Few Users (Fast-Track AOT)
**Recommendation:** **Option 1 - Extension Method Pattern**

**Rationale:**
- Small user base can upgrade quickly
- Clean break from reflection
- Simpler codebase going forward

**Action Plan:**
1. Release v2.0 with breaking changes
2. Provide migration guide
3. Offer migration support

---

#### Scenario 4: Library with Strict Backward Compatibility
**Recommendation:** **Option 5 - Hybrid Approach** (Long-term)

**Rationale:**
- Maintain compatibility for years
- Support both old and new projects
- Users choose when to migrate

**Action Plan:**
1. Implement Option 5
2. Maintain both paths for multiple major versions
3. Eventually deprecate reflection (e.g., v5.0+)

---

### Overall Recommendation

For **Lambda.Host**, I recommend:

### **Primary: Option 1 (Extension Method Pattern)**
### **Fallback: Option 5 (Hybrid) if breaking changes are unacceptable**

**Why Option 1:**
1. ✅ Lambda.Host appears to be early-stage (based on minimal README, active development)
2. ✅ Clean architecture is more important than backward compatibility at this stage
3. ✅ Sets up project for long-term success
4. ✅ Users of AOT-focused libraries expect modern patterns
5. ✅ One line of code change (`builder.Services.AddLambdaHostServices()`) is minimal

**Why Not Option 2:**
- ❌ Cross-assembly partial class complexity not worth the "automatic" behavior
- ❌ Harder to debug and explain

**Why Not Option 3:**
- ❌ Module initializers with type storage still requires runtime reflection
- ❌ Not truly AOT-safe

**Why Not Option 4:**
- ❌ Adds unnecessary indirection without benefits

**Why Consider Option 5:**
- ✅ If you have a significant user base that would be disrupted by breaking changes
- ✅ If you want to de-risk the migration
- ⚠️ At the cost of maintainability (two code paths)

---

## Implementation Roadmap

### Recommended Roadmap (Option 1)

#### **Phase 1: Foundation (Week 1)**

**Goals:**
- ✅ Update source generator to emit extension methods
- ✅ Remove assembly scanning from LambdaApplicationBuilder
- ✅ Update example projects

**Tasks:**
1. Create `Templates/LambdaHostServiceExtensions.scriban` template
2. Update `MapHandlerSourceOutput.Generate()` to emit extension method
3. Update `GeneratorConstants.cs` with new template name
4. Modify `LambdaApplicationBuilder.Build()` - remove assembly scanning
5. Update `examples/Lambda.Host.Example.HelloWorld/Program.cs`

**Validation:**
- ✅ Example project builds without warnings
- ✅ Generated extension method appears in IntelliSense
- ✅ Lambda function executes successfully

---

#### **Phase 2: Testing (Week 1-2)**

**Goals:**
- ✅ Comprehensive test coverage
- ✅ AOT compilation verification

**Tasks:**
1. Add unit tests for extension method generation
2. Add integration tests for service registration
3. Test AOT compilation with `dotnet publish -c Release /p:PublishAot=true`
4. Verify no AOT warnings in build output
5. Performance testing (cold start time comparison)

**Success Criteria:**
- ✅ All existing tests pass
- ✅ New tests achieve >90% coverage of changes
- ✅ Zero AOT warnings

---

#### **Phase 3: Documentation (Week 2)**

**Goals:**
- ✅ Clear migration guide
- ✅ Updated README
- ✅ Breaking changes document

**Tasks:**
1. Create `MIGRATION.md` with before/after examples
2. Update `README.md` with AOT section
3. Add XML documentation comments to generated extension method
4. Create `BREAKING_CHANGES.md` for v2.0 release
5. Update GitHub issues/discussions with migration plan

**Deliverables:**
- ✅ Migration guide
- ✅ Updated README
- ✅ Breaking changes document
- ✅ Blog post (optional)

---

#### **Phase 4: Release (Week 3)**

**Goals:**
- ✅ Release v2.0.0 with breaking changes
- ✅ Communicate changes to users

**Tasks:**
1. Update CHANGELOG.md
2. Bump version to 2.0.0
3. Create GitHub release with migration notes
4. Publish NuGet package
5. Monitor GitHub issues for migration support

**Communication:**
```markdown
## Lambda.Host v2.0.0 - AOT Compatibility Release

### Breaking Changes
Lambda.Host now supports .NET Native AOT! This requires a small code change:

**Before:**
```csharp
var builder = LambdaApplication.CreateBuilder();
var lambda = builder.Build();
```

**After:**
```csharp
var builder = LambdaApplication.CreateBuilder();
builder.Services.AddLambdaHostServices(); // Add this line
var lambda = builder.Build();
```

See [MIGRATION.md](MIGRATION.md) for details.
```

---

### Alternative Roadmap (Option 5 - Hybrid)

#### **Phase 1: Foundation (Week 1)**

**Goals:**
- ✅ Add feature flag support
- ✅ Implement conditional extension method generation
- ✅ Maintain backward compatibility

**Tasks:**
1. Add `LambdaHostUseSourceGeneration` MSBuild property
2. Update source generator to check feature flag
3. Emit extension method only when flag is enabled
4. Add `#if !LAMBDA_HOST_USE_SOURCE_GENERATION` to Build()
5. Create example project with feature flag enabled

---

#### **Phase 2: Documentation & Testing (Week 1-2)**

**Goals:**
- ✅ Document both modes
- ✅ Test both code paths

**Tasks:**
1. Document feature flag in README
2. Test reflection mode (default)
3. Test source generation mode (opt-in)
4. Test AOT compilation with flag enabled
5. Create migration guide for opting in

---

#### **Phase 3: Release v1.5.0 (Week 2)**

**Goals:**
- ✅ Release non-breaking version with opt-in AOT support

**Tasks:**
1. Release as v1.5.0 (minor version)
2. Mark reflection path as "will be deprecated in v2.0"
3. Encourage users to opt-in and provide feedback

---

#### **Phase 4: Deprecation v2.0.0 (3-6 months later)**

**Goals:**
- ✅ Make source generation the default
- ✅ Deprecate reflection

**Tasks:**
1. Flip default: `LambdaHostUseSourceGeneration` defaults to `true`
2. Emit deprecation warnings when using reflection
3. Release as v2.0.0 (major version)

---

#### **Phase 5: Removal v3.0.0 (6-12 months later)**

**Goals:**
- ✅ Remove reflection code entirely

**Tasks:**
1. Remove `#if` conditionals
2. Remove reflection-based assembly scanning
3. Release as v3.0.0

---

## Appendix A: Code Examples

### Full Example - Option 1 Implementation

**Generated Extension Method:**
```csharp
// File: obj/Generated/Lambda.Host.SourceGenerators/.../LambdaHostServiceExtensions.g.cs
// <auto-generated>
//     Generated by the Lambda.Host source generator.
// </auto-generated>

#nullable enable

namespace Lambda.Host.Example.HelloWorld;

/// <summary>
/// Extension methods for registering Lambda.Host services.
/// Generated at compile-time from your MapHandler invocations.
/// </summary>
public static class LambdaHostServiceExtensions
{
    /// <summary>
    /// Registers Lambda.Host services with the dependency injection container.
    /// This method is auto-generated based on your MapHandler calls.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static global::Microsoft.Extensions.DependencyInjection.IServiceCollection AddLambdaHostServices(
        this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)
    {
        // Register the generated LambdaStartupService
        services.AddSingleton<LambdaStartupService>();

        // Register as IHostedService
        services.AddSingleton<global::Microsoft.Extensions.Hosting.IHostedService>(
            serviceProvider => serviceProvider.GetRequiredService<LambdaStartupService>()
        );

        return services;
    }
}
```

**User Code:**
```csharp
using Lambda.Host;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder();

// Register any additional services
builder.Services.AddSingleton<IMyService, MyService>();

// Add generated Lambda.Host services
builder.Services.AddLambdaHostServices();

var lambda = builder.Build();

lambda.MapHandler(([Request] string input, IMyService service) =>
{
    return service.Process(input);
});

await lambda.RunAsync();
```

---

## Appendix B: Performance Considerations

### Cold Start Time Comparison

**Reflection-based (current):**
```
Assembly scanning: ~10-50ms (depending on assembly size)
Service registration: ~1-5ms
Total overhead: ~15-55ms per cold start
```

**Source generation-based (Option 1):**
```
Assembly scanning: 0ms (no scanning)
Service registration: ~1-5ms (same)
Total overhead: ~1-5ms per cold start
```

**Estimated improvement:** **10-50ms faster cold starts**

For AWS Lambda, where cold starts are critical, this is significant:
- ✅ Faster cold starts = better user experience
- ✅ Lower cold start percentage in request mix
- ✅ Potential cost savings (fewer function invocations timeout)

### Binary Size Comparison

**Reflection-based:**
- Includes full reflection metadata
- Larger binary size (reflection-heavy assemblies)

**AOT with source generation:**
- Trimmed binary (unused code removed)
- No reflection metadata
- Smaller binary size

**Estimated reduction:** 20-40% smaller deployment package

---

## Appendix C: Real-World Examples

### ASP.NET Core Minimal API (Request Delegate Generator)

ASP.NET Core faced the same problem with `MapGet`, `MapPost`, etc. They solved it with the Request Delegate Generator (RDG):

**User writes:**
```csharp
app.MapGet("/api/users/{id}", (int id, UserService service) => service.GetUser(id));
```

**Generator creates:**
```csharp
// Interceptor that replaces MapGet call with optimized AOT-friendly version
[InterceptsLocation(...)]
static RequestDelegate CreateHandler(...)
{
    // Generated request delegate with compile-time type knowledge
}
```

**Lesson:** Microsoft chose source generation + interceptors for minimal API handlers in .NET 8+.

---

### Entity Framework Core

EF Core DbContext compilation:

**User writes:**
```csharp
services.AddDbContext<MyDbContext>();
```

**Generated (with source generators):**
```csharp
// DbContext configuration generated at compile time
public class MyDbContextModelBuilder : IModelCustomizer
{
    // ... compiled model
}
```

**Lesson:** EF Core moved to compile-time model building for AOT (EF Core 7+).

---

### System.Text.Json

JSON serialization:

**User writes:**
```csharp
[JsonSerializable(typeof(MyModel))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
```

**Generated:**
```csharp
// Metadata for AOT-compatible serialization
```

**Lesson:** Source generators replaced runtime reflection for JSON serialization.

---

## Appendix D: Migration Checklist

### Pre-Migration

- [ ] Review current Lambda.Host usage across projects
- [ ] Identify all usages of `LambdaApplicationBuilder`
- [ ] Create backup branch
- [ ] Update to latest Lambda.Host version

### Migration (Option 1)

- [ ] Update Lambda.Host NuGet package to v2.0.0
- [ ] Add `builder.Services.AddLambdaHostServices()` call after creating builder
- [ ] Build project and fix any compilation errors
- [ ] Run tests to verify functionality
- [ ] Test locally with AWS Lambda Mock Test Tool
- [ ] Deploy to test environment
- [ ] Monitor for issues

### Verification

- [ ] Check build output for AOT warnings
- [ ] Verify cold start times improved
- [ ] Confirm Lambda function executes successfully
- [ ] Load test to verify performance
- [ ] Check CloudWatch logs for errors

### Optional: Enable Full AOT

- [ ] Add `<PublishAot>true</PublishAot>` to `.csproj`
- [ ] Run `dotnet publish -c Release`
- [ ] Verify no AOT warnings
- [ ] Test deployed AOT binary
- [ ] Compare binary size and cold start times

---

## Appendix E: FAQ

**Q: Why can't reflection work with AOT?**

A: AOT requires all types to be known at compile time. Reflection APIs like `Assembly.GetTypes()` require runtime type enumeration, which is incompatible with AOT's static analysis.

---

**Q: Will this break my existing code?**

A: For Option 1 (recommended): Yes, you'll need to add one line: `builder.Services.AddLambdaHostServices()`.
For Option 5 (hybrid): No, existing code continues to work.

---

**Q: Can I use both modes?**

A: With Option 5 (hybrid), yes. You can use reflection-based registration by default and opt into source generation per project.

---

**Q: What if I have multiple MapHandler calls?**

A: The current source generator already validates only one `MapHandler` call per project (see `Diagnostics.MultipleMethodCalls`). This won't change.

---

**Q: Will this work with NuGet packages?**

A: Yes, source generators work seamlessly with NuGet packages. The generator will run when the consuming project builds.

---

**Q: How do I know the extension method was generated?**

A: Check your `obj/Generated/` directory (if `EmitCompilerGeneratedFiles` is enabled), or use IntelliSense - the extension method will appear after typing `builder.Services.`.

---

**Q: What about AOT warnings?**

A: With Option 1, there should be zero AOT warnings. If you see warnings, please file an issue.

---

**Q: Can I customize the generated code?**

A: Not directly, but you can:
1. Modify the Scriban templates in `Lambda.Host.SourceGenerators/Templates/`
2. Fork the project and adjust generator logic

---

**Q: Will this work with Dependency Injection frameworks like Autofac?**

A: Yes, the generated extension method works with any `IServiceCollection`-compatible framework.

---

## Conclusion

Lambda.Host has a solid foundation with existing source generators. The only barrier to AOT compatibility is reflection-based service discovery in `LambdaApplicationBuilder.Build()`.

**Recommended Path Forward:**

1. **Short-term:** Implement **Option 1 (Extension Method Pattern)** for clean AOT support
2. **Alternative:** Implement **Option 5 (Hybrid)** if backward compatibility is critical

**Next Steps:**

1. Review this document with the team
2. Decide on Option 1 or Option 5
3. Follow implementation roadmap
4. Release v2.0.0 (Option 1) or v1.5.0 (Option 5)
5. Monitor user feedback and iterate

**Expected Outcomes:**

✅ Full .NET Native AOT support
✅ 10-50ms faster cold starts
✅ 20-40% smaller deployment packages
✅ Modern, maintainable architecture
✅ Industry-standard patterns

---

**Questions or feedback?** Open a GitHub issue or discussion.

**Ready to implement?** See the [Implementation Roadmap](#implementation-roadmap) section.

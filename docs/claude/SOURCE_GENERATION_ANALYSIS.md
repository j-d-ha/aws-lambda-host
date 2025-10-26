# Source Generation Failure Analysis: Lambda.Host.Example.HelloWorld

## Executive Summary

The Lambda.Host.Example.HelloWorld project successfully compiles but **does not trigger source generation** from Lambda.Host.SourceGenerators despite proper references. This analysis identifies the root cause and provides troubleshooting options.

## Problem Statement

When building `Lambda.Host.Example.HelloWorld`, the source generator from `Lambda.Host.SourceGenerators` does not execute, resulting in:
- No generated files in `obj/Generated/` directory (folder exists but is empty)
- No generated files in `obj/Debug/net8.0/generated/` directory (does not exist)
- Build succeeds with no errors or warnings
- Runtime would fail due to missing generated code

## Architecture Overview

```
Lambda.Host.Example.HelloWorld (net8.0)
    └── ProjectReference: Lambda.Host
            └── ProjectReference: Lambda.Host.SourceGenerators
                    (ReferenceOutputAssembly=false, OutputItemType=Analyzer)
```

### Project Configuration Files

#### Lambda.Host.Example.HelloWorld.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
    <CompilerGeneratedFilesOutputPath>obj/Generated</CompilerGeneratedFilesOutputPath>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Lambda.Host\Lambda.Host.csproj"/>
  </ItemGroup>
</Project>
```

#### Lambda.Host.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Lambda.Host.SourceGenerators\Lambda.Host.SourceGenerators.csproj"
                      ReferenceOutputAssembly="false"
                      OutputItemType="Analyzer"/>
  </ItemGroup>
</Project>
```

#### Lambda.Host.SourceGenerators.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <EnforceExtendedAnalyzerRules>true</EnforceExtendedAnalyzerRules>
    <IsRoslynComponent>true</IsRoslynComponent>
    <IncludeBuildOutput>false</IncludeBuildOutput>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.14.0" PrivateAssets="all"/>
  </ItemGroup>
</Project>
```

## Root Cause Analysis

### Primary Issue: **Transitive Analyzer References Are Not Propagated**

The source generator is correctly referenced by `Lambda.Host` with `OutputItemType="Analyzer"`, but this analyzer reference **does not flow transitively** to `Lambda.Host.Example.HelloWorld`.

#### Why This Happens

1. **MSBuild Analyzer Propagation Rules**:
   - Project references with `OutputItemType="Analyzer"` are **NOT transitive by design**
   - Only the immediate project (`Lambda.Host`) sees the analyzer
   - Consuming projects (`Lambda.Host.Example.HelloWorld`) do **not** inherit analyzer references

2. **Verification from Build Output**:
   - Diagnostic build output shows only SDK analyzers are loaded:
     ```
     Analyzer
         /usr/local/share/dotnet/sdk/.../analyzers/Microsoft.CodeAnalysis.CSharp.NetAnalyzers.dll
         /usr/local/share/dotnet/sdk/.../analyzers/Microsoft.CodeAnalysis.NetAnalyzers.dll
     ```
   - No mention of `Lambda.Host.SourceGenerators.dll` being loaded

3. **Generator Expectations**:
   - The generator looks for `MapHandler` method calls in the **consuming project's code**
   - Location: `MapHandlerIncrementalGenerator.cs:11-18`
   - Syntax provider searches for invocations matching `MapHandler` method name
   - Semantic validation confirms the method is from `LambdaApplication` type (line 65)

### Code Analysis: What the Generator Expects

From `Program.cs:10`:
```csharp
lambda.MapHandler((ILambdaContext ctx1) => "hello world");
```

The generator should:
1. Detect this invocation via `MapHandlerSyntaxProvider.Predicate()`
2. Extract lambda parameter info via `ExtractInfoFromLambda()`
3. Generate a `LambdaStartup` service class
4. Output to `obj/Generated/Lambda.Host.SourceGenerators/...`

**But this never happens because the generator isn't loaded for the HelloWorld project.**

## Evidence Supporting the Root Cause

### 1. Empty Generated Directory
```bash
$ ls -la examples/Lambda.Host.Example.HelloWorld/obj/Generated/
total 0
drwxr-xr-x@  2 jonasha  staff   64 Oct  1 21:25 .
```

### 2. No Generator References in Build
Searching diagnostic build output for "Lambda.Host.SourceGenerators" shows it's only referenced as a project dependency, never as a loaded analyzer.

### 3. Project Reference Chain
The `Lambda.Host` project correctly references the generator, but diagnostic output confirms this reference is scoped to `Lambda.Host` only.

## Troubleshooting Options

### Option 1: Direct Analyzer Reference (Recommended for Development)

**Add direct project reference to the source generator in HelloWorld project:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
    <CompilerGeneratedFilesOutputPath>obj/Generated</CompilerGeneratedFilesOutputPath>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Amazon.Lambda.RuntimeSupport" Version="1.13.4"/>
    <PackageReference Include="Amazon.Lambda.Core" Version="2.7.1"/>
    <PackageReference Include="Amazon.Lambda.Serialization.SystemTextJson" Version="2.4.4"/>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Lambda.Host\Lambda.Host.csproj"/>

    <!-- ADD THIS: Direct analyzer reference -->
    <ProjectReference Include="..\..\src\Lambda.Host.SourceGenerators\Lambda.Host.SourceGenerators.csproj"
                      ReferenceOutputAssembly="false"
                      OutputItemType="Analyzer"/>
  </ItemGroup>
</Project>
```

**Pros:**
- Simple and immediate
- Works for local development
- Clear and explicit

**Cons:**
- Requires consumers to manually add the analyzer reference
- Not ideal for packaged distribution
- Duplicates reference configuration

### Option 2: Package the Generator with Lambda.Host (Recommended for Distribution)

**Modify Lambda.Host.csproj to include the analyzer in the package:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <IncludeBuildOutput>true</IncludeBuildOutput>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Amazon.Lambda.Core" Version="2.7.1"/>
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.1"/>
  </ItemGroup>

  <!-- Package the source generator with this library -->
  <ItemGroup>
    <ProjectReference Include="..\Lambda.Host.SourceGenerators\Lambda.Host.SourceGenerators.csproj"
                      ReferenceOutputAssembly="false"
                      OutputItemType="Analyzer"
                      Pack="true"
                      PrivateAssets="none"/>
  </ItemGroup>

  <!-- Ensure analyzer is included in the NuGet package -->
  <ItemGroup>
    <None Include="..\Lambda.Host.SourceGenerators\bin\$(Configuration)\netstandard2.0\Lambda.Host.SourceGenerators.dll"
          Pack="true"
          PackagePath="analyzers/dotnet/cs"
          Visible="false" />
  </ItemGroup>
</Project>
```

**Additional:** Create `Lambda.Host.props` in build directory:

```xml
<!-- src/Lambda.Host/build/Lambda.Host.props -->
<Project>
  <ItemGroup>
    <Analyzer Include="$(MSBuildThisFileDirectory)../analyzers/dotnet/cs/Lambda.Host.SourceGenerators.dll" />
  </ItemGroup>
</Project>
```

**Pros:**
- Analyzers automatically flow to consumers when package is referenced
- Industry-standard approach (used by System.Text.Json, EF Core, etc.)
- Works with both project and NuGet references
- No action required from consumers

**Cons:**
- More complex packaging configuration
- Requires understanding of NuGet analyzer packaging conventions

### Option 3: MSBuild Properties to Force Transitive Analyzers

**Add to Lambda.Host.csproj:**

```xml
<PropertyGroup>
  <GetTargetPathDependsOn>
    $(GetTargetPathDependsOn);
    GetAnalyzerPackFiles
  </GetTargetPathDependsOn>
</PropertyGroup>

<Target Name="GetAnalyzerPackFiles" Returns="@(Analyzer)">
  <ItemGroup>
    <Analyzer Include="$(OutputPath)Lambda.Host.SourceGenerators.dll" />
  </ItemGroup>
</Target>
```

**Pros:**
- Attempts to work around MSBuild's default behavior
- Doesn't require package changes

**Cons:**
- Hacky and fragile
- May break with SDK updates
- Not guaranteed to work across all build scenarios

### Option 4: Standalone Analyzer NuGet Package

**Create a separate NuGet package for just the source generator:**

1. Create `Lambda.Host.SourceGenerators.nuspec` or modify `.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <IsRoslynComponent>true</IsRoslynComponent>
    <IncludeBuildOutput>false</IncludeBuildOutput>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <PackageId>Lambda.Host.SourceGenerators</PackageId>
    <DevelopmentDependency>true</DevelopmentDependency>
  </PropertyGroup>

  <ItemGroup>
    <None Include="$(OutputPath)\$(AssemblyName).dll"
          Pack="true"
          PackagePath="analyzers/dotnet/cs"
          Visible="false" />
  </ItemGroup>
</Project>
```

2. Have consumers install both packages:
```xml
<PackageReference Include="Lambda.Host" Version="1.0.0" />
<PackageReference Include="Lambda.Host.SourceGenerators" Version="1.0.0" PrivateAssets="all" />
```

**Pros:**
- Clear separation of concerns
- Consumers can opt-in/out of source generation
- Easier versioning independence

**Cons:**
- Requires installing two packages
- More complexity in documentation
- May confuse users

## Recommended Solution

**For Current Development:** Use **Option 1** (Direct Analyzer Reference) immediately to unblock development and testing.

**For Production/Distribution:** Implement **Option 2** (Package with Lambda.Host) as this is the industry-standard approach used by:
- System.Text.Json (JsonSerializerContext source generators)
- Entity Framework Core (DbContext source generators)
- ASP.NET Core (minimal API source generators)
- Blazor (component source generators)

This ensures consumers automatically get source generation when they reference Lambda.Host without any additional configuration.

## Verification Steps

After implementing a solution, verify with:

### 1. Clean and Rebuild
```bash
dotnet clean
dotnet build examples/Lambda.Host.Example.HelloWorld/Lambda.Host.Example.HelloWorld.csproj
```

### 2. Check for Generated Files
```bash
find examples/Lambda.Host.Example.HelloWorld/obj -name "*.g.cs"
```

Expected output:
```
examples/Lambda.Host.Example.HelloWorld/obj/Generated/Lambda.Host.SourceGenerators/Lambda.Host.SourceGenerators.MapHandlerIncrementalGenerator/LambdaStartup.g.cs
```

### 3. Verify Generated Code Content
The generated file should contain:
- A `LambdaStartup` class in the project's namespace
- `AddLambdaHost` method registering handlers
- Proper parameter extraction and dependency injection setup

### 4. Build with Verbosity
```bash
dotnet build -v detailed examples/Lambda.Host.Example.HelloWorld/Lambda.Host.Example.HelloWorld.csproj 2>&1 | grep -i "Lambda.Host.SourceGenerators.dll"
```

Should show the analyzer being loaded:
```
Analyzer
    .../Lambda.Host.SourceGenerators.dll
```

### 5. Check EmitCompilerGeneratedFiles Output
```bash
ls -la examples/Lambda.Host.Example.HelloWorld/obj/Generated/
```

Should contain subdirectories and `.g.cs` files.

## Additional Considerations

### Performance Impact
- Source generators run during compilation
- For large projects with many `MapHandler` calls, consider:
  - Incremental generator optimizations (already implemented)
  - Build caching strategies
  - Conditional compilation symbols

### IDE Support
- Visual Studio: Analyzers should appear in Dependencies > Analyzers
- Rider: Should show in External Libraries > Analyzers
- VS Code: Requires OmniSharp restart after analyzer changes

### Debugging the Generator
If issues persist after fixing the reference:

1. **Enable Generator Logging:**
```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>obj/Generated</CompilerGeneratedFilesOutputPath>
  <ReportAnalyzer>true</ReportAnalyzer>
</PropertyGroup>
```

2. **Check Generator Diagnostics:**
The generator implements diagnostics in `Diagnostics.cs`. Look for:
- Multiple parameters of the same type errors
- Unsupported delegate type warnings

3. **Attach Debugger to Source Generator:**
```xml
<PropertyGroup>
  <WaitForDebugger>true</WaitForDebugger>
</PropertyGroup>
```

This will pause the build allowing you to attach a debugger to the compiler process.

## Conclusion

The source generation failure is caused by MSBuild's design where analyzer references don't flow transitively through project references. The recommended solution is to package the source generator with Lambda.Host following NuGet analyzer packaging conventions, ensuring it's automatically available to all consumers.

This is a common pattern in the .NET ecosystem and, once implemented, will provide a seamless developer experience for Lambda.Host consumers.

---

**Document Version:** 1.0
**Date:** 2025-10-01
**Author:** Analysis by Claude Code

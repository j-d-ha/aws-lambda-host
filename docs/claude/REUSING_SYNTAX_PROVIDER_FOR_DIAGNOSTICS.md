# Reusing MapHandlerSyntaxProvider for Diagnostics

## Overview

Your `MapHandlerIncrementalGenerator` uses `MapHandlerSyntaxProvider` to collect and transform `MapHandler` invocations. Currently, diagnostics are emitted inside the source generator's output phase (`MapHandlerSourceOutput.ValidateGeneratorData`), but best practices recommend separating diagnostics into a dedicated analyzer.

This document outlines how to refactor your code to reuse `MapHandlerSyntaxProvider` in `MapHandlerAnalyzer` while maintaining the existing source generation logic.

## Problem

- **Current state**: Diagnostics are created in `MapHandlerSourceOutput.Generate` (lines 56-61, 195-248)
- **Desired state**: Diagnostics should be emitted from `MapHandlerAnalyzer` using the same syntax provider logic
- **Challenge**: Analyzers use `AnalysisContext` while source generators use `IncrementalGeneratorInitializationContext`

## Solution Architecture

The key is to **extract the core logic** from `MapHandlerSyntaxProvider` into reusable helper methods that work with both contexts.

### Current Flow
```
MapHandlerIncrementalGenerator
  └─> MapHandlerSyntaxProvider (Predicate + Transformer)
      └─> MapHandlerSourceOutput.Generate
          └─> ValidateGeneratorData (emits diagnostics)
```

### Target Flow
```
MapHandlerIncrementalGenerator
  └─> MapHandlerSyntaxProvider (Predicate + Transformer)
      └─> MapHandlerSourceOutput.Generate (no diagnostics)

MapHandlerAnalyzer
  └─> MapHandlerSyntaxProvider (reused logic)
      └─> Emit diagnostics directly
```

## Required Changes

### 1. Extract Core Syntax Logic into Helper Class

**File**: `src/Lambda.Host.SourceGenerators/MapHandlerSyntaxHelper.cs` (new file)

**Purpose**: Provide reusable methods that work with both `SyntaxNode` and `SemanticModel` directly.

**Actions**:
- Create a new static class `MapHandlerSyntaxHelper`
- Move the following methods from `MapHandlerSyntaxProvider`:
  - `Predicate(SyntaxNode, CancellationToken)` → Keep as-is (already reusable)
  - Extract semantic analysis logic into:
    - `IsMapHandlerInvocation(InvocationExpressionSyntax, SemanticModel)`
    - `TryGetMapHandlerInfo(InvocationExpressionSyntax, SemanticModel, CancellationToken, out MapHandlerInvocationInfo?)`

**Why**: These helpers can be called from both the incremental generator and the analyzer.

### 2. Update MapHandlerSyntaxProvider to Use Helpers

**File**: `src/Lambda.Host.SourceGenerators/MapHandlerSyntaxProvider.cs`

**Actions**:
- Keep `Predicate` method unchanged (it's already static and reusable)
- Update `Transformer` to delegate to `MapHandlerSyntaxHelper.TryGetMapHandlerInfo`
- This maintains backward compatibility with the generator

**Example**:
```csharp
internal static MapHandlerInvocationInfo? Transformer(
    GeneratorSyntaxContext context,
    CancellationToken token)
{
    if (context.Node is not InvocationExpressionSyntax invocation)
        return null;

    return MapHandlerSyntaxHelper.TryGetMapHandlerInfo(
        invocation,
        context.SemanticModel,
        token,
        out var info)
            ? info
            : null;
}
```

### 3. Implement MapHandlerAnalyzer with Syntax Actions

**File**: `src/Lambda.Host.SourceGenerators/MapHandlerAnalyzer.cs`

**Actions**:
- Register a `CompilationStartAction` to track all `MapHandler` invocations across the compilation
- Register a `SyntaxNodeAction` for `InvocationExpressionSyntax`
- In the syntax action:
  1. Use `MapHandlerSyntaxProvider.Predicate` to filter candidates
  2. Use `MapHandlerSyntaxHelper.TryGetMapHandlerInfo` to extract invocation info
  3. Collect all invocations in a compilation-scoped collection
- Register a `CompilationEndAction` to validate collected invocations and emit diagnostics

**Key Pattern**:
```csharp
public override void Initialize(AnalysisContext context)
{
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();

    context.RegisterCompilationStartAction(compilationContext =>
    {
        var invocations = new ConcurrentBag<MapHandlerInvocationInfo>();

        compilationContext.RegisterSyntaxNodeAction(syntaxContext =>
        {
            var node = syntaxContext.Node;

            // Reuse predicate
            if (!MapHandlerSyntaxProvider.Predicate(node, syntaxContext.CancellationToken))
                return;

            // Reuse helper to get info
            if (MapHandlerSyntaxHelper.TryGetMapHandlerInfo(
                (InvocationExpressionSyntax)node,
                syntaxContext.SemanticModel,
                syntaxContext.CancellationToken,
                out var info))
            {
                invocations.Add(info);
            }
        }, SyntaxKind.InvocationExpression);

        compilationContext.RegisterCompilationEndAction(endContext =>
        {
            // Validate and emit diagnostics
            ValidateAndReportDiagnostics(endContext, invocations);
        });
    });
}
```

### 4. Move Validation Logic from Source Output to Analyzer

**Files**:
- `src/Lambda.Host.SourceGenerators/MapHandlerSourceOutput.cs`
- `src/Lambda.Host.SourceGenerators/MapHandlerAnalyzer.cs`

**Actions**:
- Extract `ValidateGeneratorData` method from `MapHandlerSourceOutput` into a new shared helper or directly into `MapHandlerAnalyzer`
- Create a new method `ValidateAndReportDiagnostics(CompilationAnalysisContext, IEnumerable<MapHandlerInvocationInfo>)`
- Move the duplicate detection logic and parameter validation logic
- **Remove** diagnostic reporting from `MapHandlerSourceOutput.Generate`

**Example**:
```csharp
private void ValidateAndReportDiagnostics(
    CompilationAnalysisContext context,
    IEnumerable<MapHandlerInvocationInfo> invocations)
{
    var invocationList = invocations.ToList();

    // Report multiple MapHandler calls (LH0001)
    foreach (var invocation in invocationList.Skip(1))
    {
        context.ReportDiagnostic(
            Diagnostic.Create(
                Diagnostics.MultipleMethodCalls,
                invocation.LocationInfo?.ToLocation(),
                "LambdaApplication.MapHandler(Delegate)"));
    }

    // Report duplicate parameter types (LH0002)
    foreach (var invocation in invocationList)
    {
        CheckForDuplicateTypeParameters(
            context,
            invocation.DelegateInfo.Parameters,
            TypeConstants.CancellationToken);

        CheckForDuplicateTypeParameters(
            context,
            invocation.DelegateInfo.Parameters,
            TypeConstants.ILambdaContext);
    }
}
```

### 5. Update Source Generator to Focus Only on Generation

**File**: `src/Lambda.Host.SourceGenerators/MapHandlerSourceOutput.cs`

**Actions**:
- Remove lines 56-61 (diagnostic validation and early return)
- Remove `ValidateGeneratorData` method (lines 195-248)
- Keep only the source generation logic

**Rationale**: With diagnostics now in the analyzer, the generator's sole responsibility is code generation.

## Benefits of This Approach

1. **Separation of Concerns**: Diagnostics are in the analyzer, generation is in the generator
2. **Code Reuse**: `MapHandlerSyntaxProvider.Predicate` and new helper methods are shared
3. **Best Practices**: Follows Roslyn guidelines for analyzer vs. generator responsibilities
4. **Performance**: Diagnostics run on every keystroke via the analyzer; generation runs only on build
5. **Better IDE Integration**: Diagnostics appear immediately in the IDE

## Files Summary

| File | Action | Description |
|------|--------|-------------|
| `MapHandlerSyntaxHelper.cs` | **CREATE** | New shared helper with reusable syntax logic |
| `MapHandlerSyntaxProvider.cs` | **MODIFY** | Update to use new helper methods |
| `MapHandlerAnalyzer.cs` | **MODIFY** | Implement full diagnostic analysis |
| `MapHandlerSourceOutput.cs` | **MODIFY** | Remove validation, keep only generation |
| `MapHandlerIncrementalGenerator.cs` | **NO CHANGE** | Works with existing `MapHandlerSyntaxProvider` |

## Implementation Order

1. Create `MapHandlerSyntaxHelper.cs` with extracted logic
2. Update `MapHandlerSyntaxProvider.cs` to use helpers (ensures no regression)
3. Implement `MapHandlerAnalyzer.cs` with full diagnostic logic
4. Remove diagnostic logic from `MapHandlerSourceOutput.cs`
5. Test both analyzer (IDE experience) and generator (build output)

## Testing Checklist

- [ ] Analyzer detects multiple `MapHandler` calls (LH0001)
- [ ] Analyzer detects duplicate parameter types (LH0002)
- [ ] Diagnostics appear immediately in IDE
- [ ] Source generation still produces correct output
- [ ] No duplicate diagnostics from both analyzer and generator
- [ ] Performance is acceptable (no lag in IDE)

# Task: Simplify ApplyTypeOverrides Architecture

## Objective
Refactor the current type override system to make `ApplyTypeOverrides` completely agnostic to lambda vs delegate expressions. The override functions should be created conditionally within a unified extraction flow, eliminating special case handling and reducing complexity.

## Current Issues with Architecture
1. **Duplicate Methods**: `ExtractInfoFromDelegate2` and `ExtractInfoFromLambda2` are essentially wrappers
2. **Complex Cast Handling**: `HandleCastExpression` has special logic for different expression types
3. **Fragmented Logic**: Type override creation is scattered across multiple methods
4. **Not Truly Agnostic**: The system still treats lambdas and delegates differently

## New Simplified Architecture

### Core Principle
- **Single extraction method** that handles all expression types (lambda, delegate, cast)
- **Conditional override creation** happens in one place based on expression analysis
- **ApplyTypeOverrides** just applies functions to baseInfo regardless of source

### Implementation Plan

#### Step 1: Create Unified Extraction Method
```csharp
private static DelegateInfo? ExtractDelegateInfo(
    GeneratorSyntaxContext context,
    ExpressionSyntax expression)
{
    // Analyze expression to determine base extraction method and overrides needed
    var (baseInfo, overrides) = AnalyzeExpression(context, expression);
    
    return baseInfo == null ? null : ApplyTypeOverrides(baseInfo, overrides);
}
```

#### Step 2: Create Expression Analysis Logic
```csharp
private static (DelegateInfo? baseInfo, Func<DelegateInfo, DelegateInfo>[] overrides) 
    AnalyzeExpression(GeneratorSyntaxContext context, ExpressionSyntax expression)
{
    return expression switch
    {
        // Direct lambda - no overrides needed
        ParenthesizedLambdaExpressionSyntax lambda => 
            (ExtractInfoFromLambda(context, lambda), Array.Empty<Func<DelegateInfo, DelegateInfo>>()),
            
        // Direct delegate - no overrides needed  
        IdentifierNameSyntax or MemberAccessExpressionSyntax => 
            (ExtractInfoFromDelegate(context, expression), Array.Empty<Func<DelegateInfo, DelegateInfo>>()),
            
        // Cast expression - extract inner + create cast override
        CastExpressionSyntax cast => HandleCastAnalysis(context, cast),
        
        _ => (null, Array.Empty<Func<DelegateInfo, DelegateInfo>>())
    };
}
```

#### Step 3: Simplify Cast Analysis
```csharp
private static (DelegateInfo? baseInfo, Func<DelegateInfo, DelegateInfo>[] overrides)
    HandleCastAnalysis(GeneratorSyntaxContext context, CastExpressionSyntax castExpr)
{
    // Extract inner expression
    var innerExpression = GetInnerExpression(castExpr);
    
    // Get base info from inner expression (recursive call to AnalyzeExpression)
    var (baseInfo, _) = AnalyzeExpression(context, innerExpression);
    
    // Create cast override function
    var castOverride = UpdateTypesFromCast(context, castExpr);
    
    return (baseInfo, new[] { castOverride });
}
```

#### Step 4: Update AnalyzeMapHandlerLambda2
```csharp
private static DelegateInfo? AnalyzeMapHandlerLambda2(
    GeneratorSyntaxContext context,
    CancellationToken token)
{
    // Validation logic (same as before)...
    
    // Single unified extraction - no special cases
    return ExtractDelegateInfo(context, firstArgument.Expression);
}
```

## Benefits of New Architecture

1. **True Agnosticism**: `ApplyTypeOverrides` and `ExtractDelegateInfo` don't care about expression type
2. **Single Extraction Path**: One method handles all cases through conditional override creation
3. **Eliminates Duplication**: No more wrapper methods like `ExtractInfoFromDelegate2`
4. **Cleaner Cast Handling**: Cast logic is isolated and uses recursion for inner expressions
5. **Easier Extension**: Adding new expression types only requires updating `AnalyzeExpression`
6. **Better Testing**: Each component can be tested independently

## Cleanup Required
- Remove `ExtractInfoFromDelegate2` and `ExtractInfoFromLambda2` wrapper methods
- Remove `HandleCastExpression` method
- Simplify `AnalyzeMapHandlerLambda2` to use single extraction path
- Keep `ApplyTypeOverrides` unchanged (it's already properly agnostic)

## Deliverables
1. **`ExtractDelegateInfo`** - Unified extraction method
2. **`AnalyzeExpression`** - Expression analysis with conditional override creation  
3. **`HandleCastAnalysis`** - Simplified cast handling with recursion
4. **Updated `AnalyzeMapHandlerLambda2`** - Simplified to use unified extraction
5. **Remove obsolete wrapper methods** - Clean up duplicate code
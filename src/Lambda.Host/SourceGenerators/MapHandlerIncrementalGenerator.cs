using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Lambda.Host.SourceGenerators;

[Generator]
public class MapHandlerIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all MapHandler method calls with lambda analysis
        var mapHandlerCalls = context
            .SyntaxProvider.CreateSyntaxProvider(
                static (node, token) => MapHandlerSyntaxProvider.Predicate(node, token),
                static (ctx, cancellationToken) =>
                    MapHandlerSyntaxProvider.Transformer(ctx, cancellationToken)
            )
            .Where(static m => m is not null)
            .Select(static (m, _) => m!);

        // Generate source when calls are found
        context.RegisterSourceOutput(
            mapHandlerCalls.Collect(),
            static (spc, calls) => MapHandlerSourceOutput.Generate(spc, calls)
        );
    }
}

internal sealed class DelegateInfo
{
    internal required string? ResponseType { get; set; } = TypeConstants.Void;
    internal required string? Namespace { get; set; }
    internal required bool IsAsync { get; set; }

    internal string DelegateType => ResponseType == TypeConstants.Void ? "Action" : "Func";
    internal List<ParameterInfo> Parameters { get; set; } = [];
}

internal sealed class ParameterInfo
{
    internal required string? ParameterName { get; set; }
    internal required string? Type { get; set; }
    internal List<AttributeInfo> Attributes { get; set; } = [];
}

internal sealed class AttributeInfo
{
    internal required string? Type { get; set; }
    internal List<string> Arguments { get; set; } = [];
}

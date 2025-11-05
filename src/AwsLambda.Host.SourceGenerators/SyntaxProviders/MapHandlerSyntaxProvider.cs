using System.Threading;
using AwsLambda.Host.SourceGenerators.Models;
using Microsoft.CodeAnalysis;

namespace AwsLambda.Host.SourceGenerators;

internal static class MapHandlerSyntaxProvider
{
    internal static bool Predicate(SyntaxNode node, CancellationToken cancellationToken) =>
        node.TryGetMethodName(out var name)
        && name == GeneratorConstants.MapHandlerMethodName
        && !node.IsGeneratedFile();

    internal static HigherOrderMethodInfo? Transformer(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken
    ) =>
        GenericHandlerInfoExtractor.Transformer(
            context,
            "MapHandler",
            IsBaseMapHandlerCall,
            cancellationToken
        );

    private static bool IsBaseMapHandlerCall(this DelegateInfo delegateInfo) =>
        delegateInfo
            is {
                FullResponseType: TypeConstants.Task,
                Parameters: [{ Type: TypeConstants.ILambdaHostContext }],
            };
}

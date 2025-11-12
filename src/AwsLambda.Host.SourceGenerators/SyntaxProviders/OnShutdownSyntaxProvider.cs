using System.Threading;
using AwsLambda.Host.SourceGenerators.Models;
using Microsoft.CodeAnalysis;

namespace AwsLambda.Host.SourceGenerators;

internal static class OnShutdownSyntaxProvider
{
    internal static bool Predicate(SyntaxNode node, CancellationToken cancellationToken) =>
        GenericHandlerInfoExtractor.Predicate(node, GeneratorConstants.OnShutdownMethodName);

    internal static HigherOrderMethodInfo? Transformer(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken
    ) =>
        GenericHandlerInfoExtractor.Transformer(
            context,
            "OnShutdown",
            IsBaseOnShutdownCall,
            cancellationToken
        );

    // we want to filter out the non-generic shutdown method calls that use the method signature
    // defined in ILambdaApplication. this is LambdaShutdownDelegate.
    // Func<IServiceProvider, CancellationToken, Task>
    private static bool IsBaseOnShutdownCall(this DelegateInfo delegateInfo) =>
        delegateInfo
            is {
                ReturnTypeInfo.FullyQualifiedType: TypeConstants.Task,
                Parameters: [
                    { Type: TypeConstants.IServiceProvider },
                    { Type: TypeConstants.CancellationToken },
                ],
            };
}

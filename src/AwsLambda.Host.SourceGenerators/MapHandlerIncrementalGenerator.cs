using AwsLambda.Host.SourceGenerators.Models;
using AwsLambda.Host.SourceGenerators.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace AwsLambda.Host.SourceGenerators;

[Generator]
public class MapHandlerIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all MapHandler method calls with lambda analysis
        var mapHandlerCalls = context
            .SyntaxProvider.CreateSyntaxProvider(
                MapHandlerSyntaxProvider.Predicate,
                MapHandlerSyntaxProvider.Transformer
            )
            .Where(static m => m is not null)
            .Select(static (m, _) => m!.Value);

        // find any calls to `UseOpenTelemetryTracing` and extract the location
        var openTelemetryTracingCalls = context
            .SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) =>
                    node.TryGetMethodName(out var name)
                    && name == GeneratorConstants.UseOpenTelemetryTracingMethodName,
                transform: static UseOpenTelemetryTracingInfo? (context, token) =>
                {
                    var operation = context.SemanticModel.GetOperation(context.Node, token);

                    if (
                        operation is IInvocationOperation targetOperation
                        && targetOperation.TargetMethod.ContainingNamespace
                            is {
                                Name: "Host",
                                ContainingNamespace:
                                { Name: "AwsLambda", ContainingNamespace.IsGlobalNamespace: true }
                            }
                        && targetOperation.TargetMethod.ContainingAssembly.Name
                            == "AwsLambda.Host.OpenTelemetry"
                    )
                    {
                        var interceptableLocation = context.SemanticModel.GetInterceptableLocation(
                            (InvocationExpressionSyntax)targetOperation.Syntax
                        )!;

                        return new UseOpenTelemetryTracingInfo(
                            LocationInfo: LocationInfo.CreateFrom(context.Node),
                            InterceptableLocationInfo: InterceptableLocationInfo.CreateFrom(
                                interceptableLocation
                            )
                        );
                    }

                    return null;
                }
            )
            .Where(static m => m is not null)
            .Select(static (m, _) => m!.Value);

        // combine the compilation and map handler calls
        var combined = mapHandlerCalls
            .Collect()
            .Combine(openTelemetryTracingCalls.Collect())
            .Select(
                (t, _) => new CompilationInfo(t.Left.ToEquatableArray(), t.Right.ToEquatableArray())
            );

        // Generate source when calls are found
        context.RegisterSourceOutput(combined, MapHandlerSourceOutput.Generate);
    }
}

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MinimalLambda.SourceGenerators.Extensions;
using WellKnownType = MinimalLambda.SourceGenerators.WellKnownTypes.WellKnownTypeData.WellKnownType;

namespace MinimalLambda.SourceGenerators.Models;

internal readonly record struct ParameterInfo2(string Assignment, string InfoComment);

internal static class ParameterInfo2Extensions
{
    private static Func<string, DiagnosticResult<ParameterInfo2>> Success(string infoComment)
    {
        var info = new ParameterInfo2 { InfoComment = infoComment };
        return assignment =>
            DiagnosticResult<ParameterInfo2>.Success(info with { Assignment = assignment });
    }

    extension(ParameterInfo2)
    {
        internal static DiagnosticResult<ParameterInfo2> CreateForInvocationHandler(
            IParameterSymbol parameter,
            GeneratorContext context
        )
        {
            var stream = context.WellKnownTypes.Get(WellKnownType.System_IO_Stream);
            var lambdaContext = context.WellKnownTypes.Get(
                WellKnownType.Amazon_Lambda_Core_ILambdaContext
            );
            var lambdaInvocationContext = context.WellKnownTypes.Get(
                WellKnownType.MinimalLambda_ILambdaInvocationContext
            );
            var cancellationToken = context.WellKnownTypes.Get(
                WellKnownType.System_Threading_CancellationToken
            );

            var paramType = parameter.Type.ToGloballyQualifiedName();

            var success = Success("");

            // event
            if (parameter.IsFromEvent(context))
                return success(
                    SymbolEqualityComparer.Default.Equals(parameter.Type, stream)
                        // stream event
                        ? "context.Features.GetRequired<IInvocationDataFeature>().EventStream"
                        // non stream event
                        : $"context.GetRequiredEvent<{paramType}>()"
                );

            // context
            if (
                SymbolEqualityComparer.Default.Equals(parameter.Type, lambdaContext)
                || SymbolEqualityComparer.Default.Equals(parameter.Type, lambdaInvocationContext)
            )
                return success("context");

            // cancellation token
            if (SymbolEqualityComparer.Default.Equals(parameter.Type, cancellationToken))
                return success("context.CancellationToken");

            // default assignment from Di
            return parameter
                .GetDiParameterAssignment(context)
                .Bind(assignment => success(assignment));
        }

        internal static DiagnosticResult<ParameterInfo2> CreateForLifecycleHandler(
            IParameterSymbol parameter,
            GeneratorContext context
        ) => throw new NotImplementedException();
    }

    extension(IParameterSymbol parameterSymbol)
    {
        private bool IsFromEvent(GeneratorContext context)
        {
            var eventAttr = context.WellKnownTypes.Get(
                WellKnownType.MinimalLambda_Builder_EventAttribute
            );
            var fromEventAttr = context.WellKnownTypes.Get(
                WellKnownType.MinimalLambda_Builder_FromEventAttribute
            );

            return parameterSymbol
                .GetAttributes()
                .Any(attribute =>
                {
                    // check event
                    if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, eventAttr))
                        return true;

                    // check from event
                    return SymbolEqualityComparer.Default.Equals(
                        attribute.AttributeClass,
                        fromEventAttr
                    );
                });
        }

        private bool IsFromKeyedService(
            GeneratorContext context,
            out DiagnosticResult<string>? keyResult
        )
        {
            keyResult = null;

            var fromKeyedServicesAttr = context.WellKnownTypes.Get(
                WellKnownType.Microsoft_Extensions_DependencyInjection_FromKeyedServicesAttribute
            );

            foreach (var attribute in parameterSymbol.GetAttributes())
            {
                if (attribute is null)
                    continue;

                var attrClass = attribute.AttributeClass;

                // check keyed service
                if (!SymbolEqualityComparer.Default.Equals(attrClass, fromKeyedServicesAttr))
                    continue;

                keyResult = attribute.ExtractKeyedServiceKey();
                return true;
            }

            return false;
        }

        private DiagnosticResult<string> GetDiParameterAssignment(GeneratorContext context)
        {
            var paramType = parameterSymbol.Type.ToGloballyQualifiedName();

            var isKeyedServices = parameterSymbol.IsFromKeyedService(context, out var keyResult);

            // keyed services
            if (isKeyedServices)
                return keyResult!.Bind(key =>
                    DiagnosticResult<string>.Success(
                        parameterSymbol.IsOptional
                            ? $"context.ServiceProvider.GetKeyedService<{paramType}>({key})"
                            : $"context.ServiceProvider.GetRequiredKeyedService<{paramType}>({key})"
                    )
                );

            return DiagnosticResult<string>.Success(
                parameterSymbol.IsOptional
                    // default - inject from DI - optional
                    ? $"context.ServiceProvider.GetService<{paramType}>()"
                    // default - inject required from DI
                    : $"context.ServiceProvider.GetRequiredService<{paramType}>()"
            );
        }
    }

    extension(AttributeData attributeData)
    {
        private DiagnosticResult<string> ExtractKeyedServiceKey()
        {
            var argument = attributeData.ConstructorArguments[0];

            if (argument.IsNull)
                return DiagnosticResult<string>.Success("null");

            object? value = null;
            try
            {
                value = argument.Value;
            }
            catch
            {
                // ignore
            }

            if (value is null)
                return DiagnosticResult<string>.Failure(
                    Diagnostics.InvalidAttributeArgument,
                    attributeData.GetAttributeArgumentLocation(0),
                    argument.Type?.ToGloballyQualifiedName()
                );

            return DiagnosticResult<string>.Success(
                argument.Kind switch
                {
                    TypedConstantKind.Primitive when value is string strValue =>
                        SymbolDisplay.FormatLiteral(strValue, true),

                    TypedConstantKind.Primitive when value is char charValue => $"'{charValue}'",

                    TypedConstantKind.Primitive when value is bool boolValue => boolValue
                        ? "true"
                        : "false",

                    TypedConstantKind.Primitive or TypedConstantKind.Enum =>
                        $"({argument.Type?.ToGloballyQualifiedName()}){value}",

                    TypedConstantKind.Type when value is ITypeSymbol typeValue =>
                        $"typeof({typeValue.ToGloballyQualifiedName()})",

                    _ => value.ToString(),
                }
            );
        }

        private LocationInfo? GetAttributeArgumentLocation(int index) =>
            attributeData.ApplicationSyntaxReference?.GetSyntax()
                is AttributeSyntax { ArgumentList: { } argumentList }
                ? argumentList
                    .Arguments.ElementAtOrDefault(index)
                    ?.Expression.GetLocation()
                    .CreateLocationInfo()
                : null;
    }
}

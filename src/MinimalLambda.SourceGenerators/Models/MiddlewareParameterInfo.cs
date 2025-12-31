using Microsoft.CodeAnalysis;
using MinimalLambda.SourceGenerators.Extensions;
using WellKnownType = MinimalLambda.SourceGenerators.WellKnownTypes.WellKnownTypeData.WellKnownType;

namespace MinimalLambda.SourceGenerators.Models;

internal record MiddlewareParameterInfo(
    string ParameterName,
    string GloballyQualifiedType,
    string GloballyQualifiedNotNullableType,
    bool FromArguments,
    bool FromServices,
    string FromServicesAssignment,
    string InfoComment,
    MapHandlerParameterSource ServiceSource,
    string? KeyedServicesKey
);

internal static class MiddlewareParameterInfoExtensions
{
    extension(MiddlewareParameterInfo)
    {
        internal static DiagnosticResult<MiddlewareParameterInfo> Create(
            IParameterSymbol parameterSymbol,
            GeneratorContext context
        )
        {
            context.ThrowIfCancellationRequested();

            // parameter name
            var name = parameterSymbol.Name;

            // globally qualified type
            var globallyQualifiedType = parameterSymbol.Type.ToGloballyQualifiedName();

            // globally qualified type - not nullable
            var globallyQualifiedNotNullableType =
                parameterSymbol.Type.ToNotNullableGloballyQualifiedName();

            // determine if it has a `[FromServices]` attribute
            var fromServices = parameterSymbol.IsDecoratedWithAttribute(
                WellKnownType.MinimalLambda_Builder_FromServicesAttribute,
                context
            );

            // determine if it has a `[FromArguments]` attribute
            var fromArguments = parameterSymbol.IsDecoratedWithAttribute(
                WellKnownType.MinimalLambda_Builder_FromServicesAttribute,
                context
            );

            // assignment from arguments

            // assignment from services
            return parameterSymbol
                .GetDiParameterAssignment(context)
                .Bind(diInfo =>
                    DiagnosticResult<MiddlewareParameterInfo>.Success(
                        new MiddlewareParameterInfo(
                            InfoComment: "",
                            ParameterName: name,
                            GloballyQualifiedType: globallyQualifiedType,
                            GloballyQualifiedNotNullableType: globallyQualifiedNotNullableType,
                            FromArguments: fromServices,
                            FromServices: fromArguments,
                            FromServicesAssignment: diInfo.Assignment,
                            ServiceSource: diInfo.Key is not null
                                ? MapHandlerParameterSource.KeyedServices
                                : MapHandlerParameterSource.Services,
                            KeyedServicesKey: diInfo.Key
                        )
                    )
                );
        }
    }
}

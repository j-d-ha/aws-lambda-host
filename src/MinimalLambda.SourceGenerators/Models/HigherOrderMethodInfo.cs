using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using LayeredCraft.SourceGeneratorTools.Types;
using Microsoft.CodeAnalysis;
using MinimalLambda.SourceGenerators.Extensions;
using MinimalLambda.SourceGenerators.WellKnownTypes;
using ParameterAssigner = System.Func<
    Microsoft.CodeAnalysis.IParameterSymbol,
    MinimalLambda.SourceGenerators.GeneratorContext,
    MinimalLambda.SourceGenerators.Models.DiagnosticResult<MinimalLambda.SourceGenerators.Models.ParameterInfo2>
>;
using WellKnownType = MinimalLambda.SourceGenerators.WellKnownTypes.WellKnownTypeData.WellKnownType;

namespace MinimalLambda.SourceGenerators.Models;

internal readonly record struct HigherOrderMethodInfo(
    string Name,
    DelegateInfo DelegateInfo,
    LocationInfo? LocationInfo,
    InterceptableLocationInfo InterceptableLocationInfo,
    ImmutableArray<ArgumentInfo> ArgumentsInfos,
    // ── New ──────────────────────────────────────────────────────────────────────────
    string DelegateCastType = "",
    EquatableArray<ParameterInfo2> ParameterAssignments = default,
    bool IsAwaitable = false,
    bool HasReturnType = false,
    bool IsReturnTypeStream = false,
    bool IsReturnTypeBool = false,
    EquatableArray<DiagnosticInfo> DiagnosticInfos = default
);

internal static class HigherOrderMethodInfoExtensions
{
    extension(HigherOrderMethodInfo)
    {
        internal static HigherOrderMethodInfo? Create(
            IMethodSymbol methodSymbol,
            GeneratorContext context
        )
        {
            var gotName = context.Node.TryGetMethodName(out var methodName);
            Debug.Assert(gotName, "Could not get method name. This should be unreachable");

            var handlerCastType = methodSymbol.GetCastableSignature();

            if (!InterceptableLocationInfo.TryGet(context, out var interceptableLocation))
                throw new InvalidOperationException("Unable to get interceptable location");

            ParameterAssigner getParameterAssignments = methodName switch
            {
                "MapHandler" => ParameterInfo2.CreateForInvocationHandler,
                "OnInit" or "OnShutdown" => ParameterInfo2.CreateForLifecycleHandler,
                _ => throw new InvalidOperationException(
                    $"Handler with name '{methodName}' is not valid"
                ),
            };

            var (assignments, diagnostics) = methodSymbol
                .Parameters.Select(parameter => getParameterAssignments(parameter, context))
                .Aggregate(
                    (
                        Successes: new List<ParameterInfo2>(),
                        Diagnostics: new List<DiagnosticInfo>()
                    ),
                    static (acc, result) =>
                    {
                        result.Do(
                            info => acc.Successes.Add(info),
                            diagnostic => acc.Diagnostics.Add(diagnostic)
                        );

                        return acc;
                    },
                    static acc =>
                        (acc.Successes.ToEquatableArray(), acc.Diagnostics.ToEquatableArray())
                );

            var isAwaitable = methodSymbol.IsAwaitable(context);
            var hasReturnType = methodSymbol.HasMeaningfulReturnType(context);
            var isReturnTypeStream =
                hasReturnType
                && context.WellKnownTypes.IsTypeMatch(
                    methodSymbol.ReturnType,
                    WellKnownType.System_IO_Stream
                );
            var isReturnTypeBool =
                hasReturnType
                && !isReturnTypeStream
                && context.WellKnownTypes.IsTypeMatch(
                    methodSymbol.ReturnType,
                    WellKnownType.System_Boolean
                );

            return new HigherOrderMethodInfo
            {
                Name = methodName!,
                DelegateInfo = default,
                LocationInfo = null,
                InterceptableLocationInfo = interceptableLocation.Value,
                ArgumentsInfos = default,
                DelegateCastType = handlerCastType,
                ParameterAssignments = assignments,
                IsAwaitable = isAwaitable,
                HasReturnType = hasReturnType,
                IsReturnTypeStream = isReturnTypeStream,
                IsReturnTypeBool = isReturnTypeBool,
                DiagnosticInfos = diagnostics,
            };
        }
    }

    extension(IMethodSymbol methodSymbol)
    {
        private string GetCastableSignature()
        {
            var returnType = methodSymbol.ReturnType.ToGloballyQualifiedName();
            var parameters = methodSymbol
                .Parameters.Select(
                    (p, i) =>
                    {
                        var type = p.Type.ToGloballyQualifiedName();
                        var defaultValue = p.IsOptional ? " = default" : "";
                        return $"{type} arg{i}{defaultValue}";
                    }
                )
                .ToArray();
            var parameterList = string.Join(", ", parameters);

            return $"{returnType} ({parameterList}) => throw null!";
        }

        private bool IsAwaitable(GeneratorContext context)
        {
            var returnType = methodSymbol.ReturnType;

            // Check for Task and Task<T>
            var task = context.WellKnownTypes.Get(WellKnownType.System_Threading_Tasks_Task);
            if (returnType.Equals(task, SymbolEqualityComparer.Default))
                return true;

            var taskOfT = context.WellKnownTypes.Get(WellKnownType.System_Threading_Tasks_Task_T);
            if (returnType.Equals(taskOfT, SymbolEqualityComparer.Default))
                return true;

            // Check for ValueTask and ValueTask<T>
            var valueTask = context.WellKnownTypes.Get(
                WellKnownType.System_Threading_Tasks_ValueTask
            );
            if (returnType.Equals(valueTask, SymbolEqualityComparer.Default))
                return true;

            var valueTaskOfT = context.WellKnownTypes.Get(
                WellKnownType.System_Threading_Tasks_ValueTask_T
            );
            if (returnType.OriginalDefinition.Equals(valueTaskOfT, SymbolEqualityComparer.Default))
                return true;

            // Check for custom awaitable pattern (has GetAwaiter method)
            return returnType
                .GetMembers("GetAwaiter")
                .OfType<IMethodSymbol>()
                .Any(m => m.Parameters.Length == 0 && !m.IsStatic);
        }

        private bool HasMeaningfulReturnType(GeneratorContext context)
        {
            var returnType = methodSymbol.ReturnType;

            var voidType = context.WellKnownTypes.Get(WellKnownType.System_Void);
            if (returnType.Equals(voidType, SymbolEqualityComparer.Default))
                return false;

            var task = context.WellKnownTypes.Get(WellKnownType.System_Threading_Tasks_Task);
            if (returnType.Equals(task, SymbolEqualityComparer.Default))
                return false;

            var valueTask = context.WellKnownTypes.Get(
                WellKnownType.System_Threading_Tasks_ValueTask
            );
            if (returnType.Equals(valueTask, SymbolEqualityComparer.Default))
                return false;

            return true;
        }
    }
}

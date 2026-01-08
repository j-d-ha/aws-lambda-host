using LayeredCraft.SourceGeneratorTools.Types;

namespace MinimalLambda.SourceGenerators.Models;

internal readonly record struct CompilationInfo(
    EquatableArray<MapHandlerMethodInfo> MapHandlerInvocationInfos,
    EquatableArray<LifecycleMethodInfo> OnShutdownInvocationInfos,
    EquatableArray<LifecycleMethodInfo> OnInitInvocationInfos,
    EquatableArray<UseMiddlewareTInfo> UseMiddlewareTInfos
);

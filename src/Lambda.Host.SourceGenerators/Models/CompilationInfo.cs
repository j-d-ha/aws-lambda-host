using System.Collections.Immutable;

namespace Lambda.Host.SourceGenerators.Models;

internal readonly record struct CompilationInfo
{
    internal required ImmutableArray<MapHandlerInvocationInfo> MapHandlerInvocationInfos { get; init; }

    internal required StartupClassInfo StartupClassInfo { get; init; }
    internal required bool CompilationHasErrors { get; init; }
}

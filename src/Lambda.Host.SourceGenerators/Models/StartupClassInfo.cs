namespace Lambda.Host.SourceGenerators.Models;

internal readonly record struct StartupClassInfo
{
    internal required string Namespace { get; init; }
    internal required string ClassName { get; init; }
}

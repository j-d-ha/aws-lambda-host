using Microsoft.CodeAnalysis;

namespace Lambda.Host.SourceGenerators;

internal static class Diagnostics
{
    internal static readonly DiagnosticDescriptor MultipleMethodCalls = new(
        "LH0001",
        "Multiple method calls detected",
        "Method '{0}' can only be invoked once per project. Remove this duplicate invocation.",
        "Usage",
        DiagnosticSeverity.Error,
        true
    );
}

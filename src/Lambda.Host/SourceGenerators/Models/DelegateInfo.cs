using Microsoft.CodeAnalysis;

namespace Lambda.Host.SourceGenerators.Models;

internal sealed class DelegateInfo
{
    internal required string? ResponseType { get; set; } = TypeConstants.Void;
    internal required string? Namespace { get; set; }
    internal required bool IsAsync { get; set; }

    internal string DelegateType => ResponseType == TypeConstants.Void ? "Action" : "Func";
    internal List<ParameterInfo> Parameters { get; set; } = [];
}

using Microsoft.CodeAnalysis;

namespace Lambda.Host.SourceGenerators.Models;

internal class MapHandlerInvocationInfo
{
    internal required DelegateInfo DelegateInfo { get; set; }
    internal required Location Location { get; set; }
}

using Microsoft.CodeAnalysis;

namespace Lambda.Host.SourceGenerators.Extensions;

internal static class TypeExtractorExtensions
{
    private static readonly SymbolDisplayFormat Format =
        SymbolDisplayFormat.FullyQualifiedFormat.AddMiscellaneousOptions(
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
        );

    internal static string GetAsGlobal(this ITypeSymbol typeSymbol) =>
        typeSymbol.ToDisplayString(Format);
}

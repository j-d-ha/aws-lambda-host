using Microsoft.CodeAnalysis;

namespace MinimalLambda.SourceGenerators.Extensions;

internal static class TypeExtractorExtensions
{
    private static readonly SymbolDisplayFormat NullableFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.AddMiscellaneousOptions(
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
        );

    private static readonly SymbolDisplayFormat NotNullableFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.AddMiscellaneousOptions(
            SymbolDisplayMiscellaneousOptions.ExpandNullable
        );

    extension(ITypeSymbol typeSymbol)
    {
        internal string QualifiedName => typeSymbol.ToDisplayString(NotNullableFormat);

        internal string QualifiedNullableName => typeSymbol.ToDisplayString(NullableFormat);
    }
}

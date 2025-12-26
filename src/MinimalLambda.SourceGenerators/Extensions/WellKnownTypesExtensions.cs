using Microsoft.CodeAnalysis;

namespace MinimalLambda.SourceGenerators.WellKnownTypes;

internal static class WellKnownTypesExtensions
{
    extension(WellKnownTypes wellKnownTypes)
    {
        internal bool IsTypeMatch(ITypeSymbol type, WellKnownTypeData.WellKnownType wellKnownType)
        {
            var foundType = wellKnownTypes.Get(wellKnownType);
            return type.Equals(foundType, SymbolEqualityComparer.Default);
        }
    }
}

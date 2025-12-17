using System.Linq;
using Microsoft.CodeAnalysis;
using MinimalLambda.SourceGenerators.Extensions;
using MinimalLambda.SourceGenerators.Types;

namespace MinimalLambda.SourceGenerators.Models;

internal readonly record struct ClassInfo(
    string GloballyQualifiedName,
    EquatableArray<ConstructorInfo> ConstructorInfos
);

internal static class ClassInfoExtensions
{
    extension(ClassInfo)
    {
        internal static ClassInfo Create(ITypeSymbol typeSymbol)
        {
            // get the globally qualified name of the class
            var globallyQualifiedName = typeSymbol.GetAsGlobal();

            // handle each instance constructor on the type
            var constructorInfo = ((INamedTypeSymbol)typeSymbol)
                .InstanceConstructors.Select(ConstructorInfo.Create)
                .ToEquatableArray();

            return new ClassInfo(globallyQualifiedName, constructorInfo);
        }
    }
}

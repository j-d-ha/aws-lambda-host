using System.Linq;
using Microsoft.CodeAnalysis;
using MinimalLambda.SourceGenerators.Extensions;
using MinimalLambda.SourceGenerators.Types;

namespace MinimalLambda.SourceGenerators.Models;

internal readonly record struct ClassInfo(
    string GloballyQualifiedName,
    string ShortName,
    EquatableArray<MethodInfo> ConstructorInfos
);

internal static class ClassInfoExtensions
{
    extension(ClassInfo)
    {
        internal static ClassInfo Create(ITypeSymbol typeSymbol)
        {
            // get the globally qualified name of the class
            var globallyQualifiedName = typeSymbol.GetAsGlobal();

            // get short name
            var shortName = typeSymbol.Name;

            // handle each instance constructor on the type
            var constructorInfo = ((INamedTypeSymbol)typeSymbol)
                .InstanceConstructors.Select(MethodInfo.Create)
                .ToEquatableArray();

            return new ClassInfo(globallyQualifiedName, shortName, constructorInfo);
        }
    }
}

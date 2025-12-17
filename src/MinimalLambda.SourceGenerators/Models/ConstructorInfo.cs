using System.Linq;
using Microsoft.CodeAnalysis;
using MinimalLambda.SourceGenerators.Types;

namespace MinimalLambda.SourceGenerators.Models;

internal readonly record struct ConstructorInfo(
    int ArgumentCount,
    EquatableArray<string> Attributes,
    EquatableArray<ParameterInfo> Parameters
);

internal static class ConstructorInfoExtensions
{
    extension(ConstructorInfo)
    {
        internal static ConstructorInfo Create(IMethodSymbol constructor)
        {
            var attributeNames = constructor
                .GetAttributes()
                .Where(a => a.AttributeClass is not null)
                .Select(a => a.AttributeClass!.ToString())
                .ToEquatableArray();

            var parameterInfos = constructor
                .Parameters.Select(ParameterInfo.Create)
                .ToEquatableArray();

            return new ConstructorInfo(parameterInfos.Count, attributeNames, parameterInfos);
        }
    }
}

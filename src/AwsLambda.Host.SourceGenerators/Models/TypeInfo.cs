using System.Collections.Immutable;
using System.Linq;
using AwsLambda.Host.SourceGenerators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AwsLambda.Host.SourceGenerators.Models;

/// <summary>Represents the information associated with a named type in C# source code.</summary>
internal readonly record struct TypeInfo(
    string FullyQualifiedType,
    string? UnwrappedFullyQualifiedType,
    bool IsGeneric,
    ImmutableArray<string> ImplementedInterfaces
);

internal static class TypeInfoExtensions
{
    extension(TypeInfo typeInfo)
    {
        internal static TypeInfo Create(ITypeSymbol typeSymbol, TypeSyntax? syntax = null)
        {
            var fullyQualifiedType = typeSymbol.GetAsGlobal(syntax);
            var unwrappedFullyQualifiedType = typeSymbol.GetUnwrappedFullyQualifiedType(syntax);
            var isGeneric = typeSymbol is INamedTypeSymbol { IsGenericType: true };
            var implementedInterfaces = typeSymbol
                .AllInterfaces.Select(i => i.GetAsGlobal())
                .ToImmutableArray();

            return new TypeInfo(
                fullyQualifiedType,
                unwrappedFullyQualifiedType,
                isGeneric,
                implementedInterfaces
            );
        }
    }

    extension(ITypeSymbol typeSymbol)
    {
        /// <summary>Gets a fully qualified type name without it being wrapped in Task or ValueTask</summary>
        private string? GetUnwrappedFullyQualifiedType(TypeSyntax? syntax = null)
        {
            if (
                typeSymbol is not INamedTypeSymbol namedTypeSymbol
                || (!namedTypeSymbol.IsTask() && !namedTypeSymbol.IsValueTask())
            )
                return typeSymbol.GetAsGlobal(syntax);

            // if not generic Task or ValueTask, return null as no wrapped return value
            if (!namedTypeSymbol.IsGenericType || namedTypeSymbol.TypeArguments.Length == 0)
                return null;

            return namedTypeSymbol.TypeArguments.First().GetAsGlobal(syntax);
        }

        /// <summary>Determines whether the type is a Task or ValueTask</summary>
        private bool IsTask() =>
            typeSymbol.Name == "Task"
            && typeSymbol.ContainingNamespace?.ToDisplayString() == "System.Threading.Tasks";

        /// <summary>Determines whether the type is a ValueTask</summary>
        private bool IsValueTask() =>
            typeSymbol.Name == "ValueTask"
            && typeSymbol.ContainingNamespace?.ToDisplayString() == "System.Threading.Tasks";
    }
}

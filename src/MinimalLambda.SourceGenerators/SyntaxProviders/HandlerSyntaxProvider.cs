using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using MinimalLambda.SourceGenerators.Models;

namespace MinimalLambda.SourceGenerators;

internal static class HandlerSyntaxProvider
{
    private static readonly string[] TargetMethodNames = ["MapHandler", "OnInit", "OnShutdown"];

    internal static bool Predicate(SyntaxNode node, CancellationToken _) =>
        !node.IsGeneratedFile()
        && node.TryGetMethodName(out var name)
        && TargetMethodNames.Contains(name);

    internal static HigherOrderMethodInfo? Transformer(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken
    ) => throw new NotImplementedException();
}

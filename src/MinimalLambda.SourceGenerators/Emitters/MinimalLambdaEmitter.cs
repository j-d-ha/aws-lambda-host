using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using MinimalLambda.SourceGenerators.Models;

namespace MinimalLambda.SourceGenerators;

internal static class MinimalLambdaEmitter
{
    internal static readonly Lazy<string> GeneratedCodeAttribute = new(() =>
    {
        var assembly = Assembly.GetExecutingAssembly();
        var generatorName = assembly.GetName().Name;
        var generatorVersion = assembly.GetName().Version;

        return $"""[global::System.CodeDom.Compiler.GeneratedCode("{generatorName}", "{generatorVersion}")]""";
    });

    internal static void Generate(SourceProductionContext context, CompilationInfo compilationInfo)
    {
        // validate the generator data and report any diagnostics before exiting.
        var diagnostics = DiagnosticGenerator.GenerateDiagnostics(compilationInfo);
        if (diagnostics.Any())
        {
            diagnostics.ForEach(context.ReportDiagnostic);

            // if there are any errors, return without generating any source code.
            if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
                return;
        }

        List<string?> outputs =
        [
            TemplateHelper.Render(
                GeneratorConstants.InterceptsLocationAttributeTemplateFile,
                new { GeneratedCodeAttribute }
            ),
            """
                namespace MinimalLambda.Generated
                {
                    using System;
                    using System.Runtime.CompilerServices;
                    using System.Threading;
                    using System.Threading.Tasks;
                    using Microsoft.Extensions.DependencyInjection;
                    using MinimalLambda;
                    using MinimalLambda.Builder;

                """,
        ];

        // if MapHandler calls found, generate the source code.
        if (compilationInfo.MapHandlerInvocationInfos.Count >= 1)
            outputs.Add(
                TemplateHelper.Render(
                    GeneratorConstants.LambdaHostMapHandlerExtensionsTemplateFile,
                    new
                    {
                        GeneratedCodeAttribute,
                        MapHandlerCalls = compilationInfo.MapHandlerInvocationInfos,
                    }
                )
            );

        // add UseMiddleware<T> interceptors
        if (compilationInfo.UseMiddlewareTInfos.Count >= 1)
            outputs.Add(
                TemplateHelper.Render(
                    GeneratorConstants.UseMiddlewareTTemplateFile,
                    new { GeneratedCodeAttribute, Calls = compilationInfo.UseMiddlewareTInfos }
                )
            );

        // add OnInit interceptors
        if (compilationInfo.OnInitInvocationInfos.Count >= 1)
            outputs.Add(
                TemplateHelper.Render(
                    GeneratorConstants.GenericHandlerTemplateFile,
                    new
                    {
                        Name = compilationInfo.OnInitInvocationInfos.First().MethodType,
                        Calls = compilationInfo.OnInitInvocationInfos,
                        GeneratedCodeAttribute,
                    }
                )
            );

        // add OnShutdown interceptors
        if (compilationInfo.OnShutdownInvocationInfos.Count >= 1)
            outputs.Add(
                TemplateHelper.Render(
                    GeneratorConstants.GenericHandlerTemplateFile,
                    new
                    {
                        Name = compilationInfo.OnShutdownInvocationInfos.First().MethodType,
                        Calls = compilationInfo.OnShutdownInvocationInfos,
                        GeneratedCodeAttribute,
                    }
                )
            );

        outputs.Add(
            """
                file static class Utilities
                {
                    internal static T Cast<T>(Delegate d, T _) where T : Delegate => (T)d;
                }
            }
            """
        );

        // join all the source code together and add it to the compilation context.
        var outCode = string.Join("\n", outputs.Where(s => s != null));

        // add the source code to the compilation context.
        context.AddSource("LambdaHandler.g.cs", outCode);
    }
}

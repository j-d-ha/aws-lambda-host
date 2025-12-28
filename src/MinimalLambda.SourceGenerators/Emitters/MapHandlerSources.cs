using LayeredCraft.SourceGeneratorTools.Types;
using MinimalLambda.SourceGenerators.Models;

namespace MinimalLambda.SourceGenerators;

internal static class MapHandlerSources
{
    internal static string Generate(EquatableArray<MapHandlerMethodInfo> mapHandlerInvocationInfos)
    {
        var template = TemplateHelper.LoadTemplate(
            GeneratorConstants.LambdaHostMapHandlerExtensionsTemplateFile
        );

        return template.Render(
            new
            {
                MinimalLambdaEmitter.GeneratedCodeAttribute,
                MapHandlerCalls = mapHandlerInvocationInfos,
            }
        );
    }
}

using System.Linq;
using MinimalLambda.SourceGenerators.Models;
using MinimalLambda.SourceGenerators.Types;

namespace MinimalLambda.SourceGenerators;

public class UseMiddlewareTSource
{
    internal static string Generate(
        EquatableArray<UseMiddlewareTInfo> useMiddlewareTInfos,
        string generatedCodeAttribute
    )
    {
        var useMiddlewareTCalls = useMiddlewareTInfos.Select(useMiddlewareTInfo =>
        {
            var classInfo = useMiddlewareTInfo.ClassInfo;

            // choose what constructor to use with the following criteria:
            // 1. if it has an `[MiddlewareConstructor]` attribute. Multiple of these are not valid.
            // 2. default to the constructor with the most arguments
            var constructor = classInfo
                .ConstructorInfos.Select(c => (MethodInfo?)c)
                .FirstOrDefault(c =>
                    c!.Value.AttributeInfos.Any(a =>
                        a.FullName == AttributeConstants.MiddlewareConstructor
                    )
                );

            constructor ??= classInfo
                .ConstructorInfos.OrderByDescending(c => c.ArgumentCount)
                .First();

            return new
            {
                Location = useMiddlewareTInfo.InterceptableLocationInfo,
                FullMiddlewareClassName = classInfo.GloballyQualifiedName,
                ShortMiddlewareClassName = classInfo.ShortName,
                constructor.Value.Parameters,
            };
        });

        var template = TemplateHelper.LoadTemplate(GeneratorConstants.UseMiddlewareTTemplateFile);

        return template.Render(
            new { GeneratedCodeAttribute = generatedCodeAttribute, Calls = useMiddlewareTCalls }
        );
    }
}

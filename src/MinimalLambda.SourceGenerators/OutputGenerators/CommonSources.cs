namespace MinimalLambda.SourceGenerators;

internal static class CommonSources
{
    internal static string Generate()
    {
        var model = new { LambdaHostOutputGenerator.GeneratedCodeAttribute };

        var template = TemplateHelper.LoadTemplate(
            GeneratorConstants.InterceptsLocationAttributeTemplateFile
        );

        return template.Render(model);
    }
}

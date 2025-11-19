namespace AwsLambda.Host;

internal class DefaultOnShutdownBuilderFactory(IServiceProvider serviceProvider)
    : IOnShutdownBuilderFactory
{
    public ILambdaOnShutdownBuilder CreateBuilder() => new LambdaOnShutdownBuilder(serviceProvider);
}

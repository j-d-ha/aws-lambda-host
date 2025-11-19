namespace AwsLambda.Host;

internal class DefaultOnInitBuilderFactory(IServiceProvider serviceProvider) : IOnInitBuilderFactory
{
    public ILambdaOnInitBuilder CreateBuilder() => new LambdaOnInitBuilder(serviceProvider);
}

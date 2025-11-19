namespace AwsLambda.Host;

internal class DefaultInvocationBuilderFactory(IServiceProvider serviceProvider)
    : IInvocationBuilderFactory
{
    public ILambdaInvocationBuilder CreateBuilder() => new LambdaInvocationBuilder(serviceProvider);
}

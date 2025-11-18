namespace AwsLambda.Host;

internal class LambdaInvocationBuilderFactory : ILambdaInvocationBuilderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public LambdaInvocationBuilderFactory(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _serviceProvider = serviceProvider;
    }

    public ILambdaInvocationBuilder CreateBuilder() =>
        new LambdaInvocationBuilder(_serviceProvider);
}

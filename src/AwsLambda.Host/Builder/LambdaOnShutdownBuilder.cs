namespace AwsLambda.Host;

internal class LambdaOnShutdownBuilder : ILambdaOnShutdownBuilder
{
    public IServiceProvider Services { get; }
    public IList<LambdaShutdownDelegate> ShutdownHandlers { get; } = [];

    public LambdaOnShutdownBuilder(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        Services = serviceProvider;
    }
}

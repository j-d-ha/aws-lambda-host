namespace AwsLambda.Host;

public interface ILambdaOnShutdownBuilder
{
    IServiceProvider Services { get; }

    List<LambdaShutdownDelegate> ShutdownHandlers { get; }

    ILambdaOnShutdownBuilder OnShutdown(LambdaShutdownDelegate handler);

    LambdaShutdownDelegate Build();
}

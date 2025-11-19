namespace AwsLambda.Host;

public interface ILambdaOnShutdownBuilder
{
    IServiceProvider Services { get; }

    IList<LambdaShutdownDelegate> ShutdownHandlers { get; }
}

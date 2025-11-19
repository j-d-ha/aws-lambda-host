namespace AwsLambda.Host;

public interface ILambdaOnInitBuilder
{
    IServiceProvider Services { get; }

    IList<LambdaInitDelegate> InitHandlers { get; }
}

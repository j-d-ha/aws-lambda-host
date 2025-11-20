namespace AwsLambda.Host;

public interface ILambdaOnInitBuilder
{
    IReadOnlyList<LambdaInitDelegate> InitHandlers { get; }
    IServiceProvider Services { get; }

    ILambdaOnInitBuilder OnInit(LambdaInitDelegate handler);

    LambdaInitDelegate Build();
}

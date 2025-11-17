namespace AwsLambda.Host;

public interface ILambdaOnInitBuilder
{
    IServiceProvider Services { get; }

    List<LambdaInitDelegate> InitHandlers { get; }

    ILambdaOnInitBuilder OnInit(LambdaInitDelegate handler);

    LambdaInitDelegate Build();
}

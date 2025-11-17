namespace AwsLambda.Host;

public interface ILambdaHandlerBuilder
{
    IServiceProvider Services { get; }

    IFeatureCollection Features { get; }

    List<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>> Middlewares { get; }

    LambdaInvocationDelegate Hanlder { get; }

    ILambdaHandlerBuilder Run(LambdaInvocationDelegate handler);

    ILambdaHandlerBuilder Use(Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware);

    LambdaInvocationDelegate Build();
}

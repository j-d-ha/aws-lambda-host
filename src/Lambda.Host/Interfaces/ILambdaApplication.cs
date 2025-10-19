using Lambda.Host.Middleware;

namespace Lambda.Host;

public interface ILambdaApplication
{
    ILambdaApplication MapHandler(Delegate handler);

    ILambdaApplication Use(Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware);
}

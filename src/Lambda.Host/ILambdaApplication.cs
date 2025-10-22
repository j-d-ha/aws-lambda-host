using Lambda.Host.Middleware;

namespace Lambda.Host;

public interface ILambdaApplication
{
    ILambdaApplication MapHandler(
        LambdaInvocationDelegate handler,
        LambdaMiddlewareDelegate? serializer = null
    );

    ILambdaApplication Use(Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware);
}

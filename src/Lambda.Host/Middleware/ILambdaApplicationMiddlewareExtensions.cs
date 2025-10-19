namespace Lambda.Host.Middleware;

public static class ILambdaApplicationMiddlewareExtensions
{
    public static ILambdaApplication UseMiddleware(
        this ILambdaApplication application,
        Func<ILambdaHostContext, LambdaInvocationDelegate, Task> middleware
    ) => application.Use(next => context => middleware(context, next));
}

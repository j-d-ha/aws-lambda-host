using Lambda.Host.Middleware;

namespace Lambda.Host;

internal sealed class DelegateHolder
{
    internal LambdaInvocationDelegate? Handler { get; set; }

    internal List<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>> Middlewares { get; } =
        [];

    internal Func<
        LambdaInvocationDelegate,
        LambdaInvocationDelegate
    >? SerializerMiddleware { get; set; }

    internal bool IsHandlerSet => Handler != null;
}

namespace Lambda.Host;

public static class LambdaHostContextExtensions
{
    public static T? GetEvent<T>(this ILambdaHostContext context)
    {
        if (context.Event is T eventT)
            return eventT;

        return default;
    }

    public static bool TryGetEvent<T>(this ILambdaHostContext context, out T? eventT) =>
        (eventT = context.GetEvent<T>()) is not null;
}

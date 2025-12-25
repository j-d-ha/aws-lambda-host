namespace System;

internal static class FunctionalExtensions
{
    extension<T, TResult>(T source)
    {
        public TResult Map(Func<T, TResult> func) => func(source);
    }

    extension<T>(T source)
    {
        public T Tap(Action<T> action)
        {
            action(source);
            return source;
        }
    }
}

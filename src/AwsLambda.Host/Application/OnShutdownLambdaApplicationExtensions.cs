using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace AwsLambda.Host;

[ExcludeFromCodeCoverage]
/// <summary>
///     Source-generated overloads for <see cref="ILambdaApplication.OnShutdown(LambdaShutdownDelegate)"/> that support
///     automatic dependency injection with zero to ten parameters.
/// </summary>
/// <remarks>
///     These methods are generated at compile time. Instead of manually resolving from <see cref="IServiceProvider"/>,
///     declare handler parameters to be automatically injected. The runtime implementations throw; they must be replaced
///     by the source generator at compile time.
/// </remarks>
/// <example>
///     <code>
///         application.OnShutdown(async (logger, database) =>
///         {
///             logger.LogInformation("Shutting down");
///             await database.FlushAsync();
///         });
///     </code>
/// </example>
public static class OnShutdownLambdaApplicationExtensions
{
    /// <summary>
    ///     Registers a shutdown handler that will be run when the Lambda runtime shuts down.
    /// </summary>
    /// <remarks>
    ///     Source generation creates the wiring code to resolve handler dependencies, using compile-time
    ///     interceptors to replace the calls. Dependencies are scoped per handler. If a CancellationToken
    ///     is requested, it will be cancelled before the Lambda runtime forces shutdown.
    /// </remarks>
    /// <param name="application">The Lambda application.</param>
    /// <param name="handler">An asynchronous handler function.</param>
    /// <returns>The current <see cref="ILambdaApplication" /> instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if called at runtime; this exception is unreachable as this method is intercepted by the
    ///     source generator code at compile time.
    /// </exception>
    public static ILambdaApplication OnShutdown(
        this ILambdaApplication application,
        Func<Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    /// <summary>
    ///     Registers a shutdown handler that will be run when the Lambda runtime shuts down.
    /// </summary>
    /// <remarks>
    ///     Source generation creates the wiring code to resolve handler dependencies, using compile-time
    ///     interceptors to replace the calls. Dependencies are scoped per handler. If a CancellationToken
    ///     is requested, it will be cancelled before the Lambda runtime forces shutdown.
    /// </remarks>
    /// <typeparam name="T1">The type of the first handler parameter.</typeparam>
    /// <param name="application">The Lambda application.</param>
    /// <param name="handler">An asynchronous handler function accepting one service parameter.</param>
    /// <returns>The current <see cref="ILambdaApplication" /> instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if called at runtime; this exception is unreachable as this method is intercepted by the
    ///     source generator code at compile time.
    /// </exception>
    public static ILambdaApplication OnShutdown<T1>(
        this ILambdaApplication application,
        Func<T1, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    /// <summary>
    ///     Registers a shutdown handler that will be run when the Lambda runtime shuts down.
    /// </summary>
    /// <remarks>
    ///     Source generation creates the wiring code to resolve handler dependencies, using compile-time
    ///     interceptors to replace the calls. Dependencies are scoped per handler. If a CancellationToken
    ///     is requested, it will be cancelled before the Lambda runtime forces shutdown.
    /// </remarks>
    /// <typeparam name="T1">The type of the first handler parameter.</typeparam>
    /// <typeparam name="T2">The type of the second handler parameter.</typeparam>
    /// <param name="application">The Lambda application.</param>
    /// <param name="handler">An asynchronous handler function accepting two service parameters.</param>
    /// <returns>The current <see cref="ILambdaApplication" /> instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if called at runtime; this exception is unreachable as this method is intercepted by the
    ///     source generator code at compile time.
    /// </exception>
    public static ILambdaApplication OnShutdown<T1, T2>(
        this ILambdaApplication application,
        Func<T1, T2, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    /// <summary>
    ///     Registers a shutdown handler that will be run when the Lambda runtime shuts down.
    /// </summary>
    /// <remarks>
    ///     Source generation creates the wiring code to resolve handler dependencies, using compile-time
    ///     interceptors to replace the calls. Dependencies are scoped per handler. If a CancellationToken
    ///     is requested, it will be cancelled before the Lambda runtime forces shutdown.
    /// </remarks>
    /// <typeparam name="T1">The type of the first handler parameter.</typeparam>
    /// <typeparam name="T2">The type of the second handler parameter.</typeparam>
    /// <typeparam name="T3">The type of the third handler parameter.</typeparam>
    /// <param name="application">The Lambda application.</param>
    /// <param name="handler">An asynchronous handler function accepting three service parameters.</param>
    /// <returns>The current <see cref="ILambdaApplication" /> instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if called at runtime; this exception is unreachable as this method is intercepted by the
    ///     source generator code at compile time.
    /// </exception>
    public static ILambdaApplication OnShutdown<T1, T2, T3>(
        this ILambdaApplication application,
        Func<T1, T2, T3, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    /// <summary>
    ///     Registers a shutdown handler that will be run when the Lambda runtime shuts down.
    /// </summary>
    /// <remarks>
    ///     Source generation creates the wiring code to resolve handler dependencies, using compile-time
    ///     interceptors to replace the calls. Dependencies are scoped per handler. If a CancellationToken
    ///     is requested, it will be cancelled before the Lambda runtime forces shutdown.
    /// </remarks>
    /// <typeparam name="T1">The type of the first handler parameter.</typeparam>
    /// <typeparam name="T2">The type of the second handler parameter.</typeparam>
    /// <typeparam name="T3">The type of the third handler parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth handler parameter.</typeparam>
    /// <param name="application">The Lambda application.</param>
    /// <param name="handler">An asynchronous handler function accepting four service parameters.</param>
    /// <returns>The current <see cref="ILambdaApplication" /> instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if called at runtime; this exception is unreachable as this method is intercepted by the
    ///     source generator code at compile time.
    /// </exception>
    public static ILambdaApplication OnShutdown<T1, T2, T3, T4>(
        this ILambdaApplication application,
        Func<T1, T2, T3, T4, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    /// <summary>
    ///     Registers a shutdown handler that will be run when the Lambda runtime shuts down.
    /// </summary>
    /// <remarks>
    ///     Source generation creates the wiring code to resolve handler dependencies, using compile-time
    ///     interceptors to replace the calls. Dependencies are scoped per handler. If a CancellationToken
    ///     is requested, it will be cancelled before the Lambda runtime forces shutdown.
    /// </remarks>
    /// <typeparam name="T1">The type of the first handler parameter.</typeparam>
    /// <typeparam name="T2">The type of the second handler parameter.</typeparam>
    /// <typeparam name="T3">The type of the third handler parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth handler parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth handler parameter.</typeparam>
    /// <param name="application">The Lambda application.</param>
    /// <param name="handler">An asynchronous handler function accepting five service parameters.</param>
    /// <returns>The current <see cref="ILambdaApplication" /> instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if called at runtime; this exception is unreachable as this method is intercepted by the
    ///     source generator code at compile time.
    /// </exception>
    public static ILambdaApplication OnShutdown<T1, T2, T3, T4, T5>(
        this ILambdaApplication application,
        Func<T1, T2, T3, T4, T5, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    /// <summary>
    ///     Registers a shutdown handler that will be run when the Lambda runtime shuts down.
    /// </summary>
    /// <remarks>
    ///     Source generation creates the wiring code to resolve handler dependencies, using compile-time
    ///     interceptors to replace the calls. Dependencies are scoped per handler. If a CancellationToken
    ///     is requested, it will be cancelled before the Lambda runtime forces shutdown.
    /// </remarks>
    /// <typeparam name="T1">The type of the first handler parameter.</typeparam>
    /// <typeparam name="T2">The type of the second handler parameter.</typeparam>
    /// <typeparam name="T3">The type of the third handler parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth handler parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth handler parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth handler parameter.</typeparam>
    /// <param name="application">The Lambda application.</param>
    /// <param name="handler">An asynchronous handler function accepting six service parameters.</param>
    /// <returns>The current <see cref="ILambdaApplication" /> instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if called at runtime; this exception is unreachable as this method is intercepted by the
    ///     source generator code at compile time.
    /// </exception>
    public static ILambdaApplication OnShutdown<T1, T2, T3, T4, T5, T6>(
        this ILambdaApplication application,
        Func<T1, T2, T3, T4, T5, T6, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    /// <summary>
    ///     Registers a shutdown handler that will be run when the Lambda runtime shuts down.
    /// </summary>
    /// <remarks>
    ///     Source generation creates the wiring code to resolve handler dependencies, using compile-time
    ///     interceptors to replace the calls. Dependencies are scoped per handler. If a CancellationToken
    ///     is requested, it will be cancelled before the Lambda runtime forces shutdown.
    /// </remarks>
    /// <typeparam name="T1">The type of the first handler parameter.</typeparam>
    /// <typeparam name="T2">The type of the second handler parameter.</typeparam>
    /// <typeparam name="T3">The type of the third handler parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth handler parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth handler parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth handler parameter.</typeparam>
    /// <typeparam name="T7">The type of the seventh handler parameter.</typeparam>
    /// <param name="application">The Lambda application.</param>
    /// <param name="handler">An asynchronous handler function accepting seven service parameters.</param>
    /// <returns>The current <see cref="ILambdaApplication" /> instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if called at runtime; this exception is unreachable as this method is intercepted by the
    ///     source generator code at compile time.
    /// </exception>
    public static ILambdaApplication OnShutdown<T1, T2, T3, T4, T5, T6, T7>(
        this ILambdaApplication application,
        Func<T1, T2, T3, T4, T5, T6, T7, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    /// <summary>
    ///     Registers a shutdown handler that will be run when the Lambda runtime shuts down.
    /// </summary>
    /// <remarks>
    ///     Source generation creates the wiring code to resolve handler dependencies, using compile-time
    ///     interceptors to replace the calls. Dependencies are scoped per handler. If a CancellationToken
    ///     is requested, it will be cancelled before the Lambda runtime forces shutdown.
    /// </remarks>
    /// <typeparam name="T1">The type of the first handler parameter.</typeparam>
    /// <typeparam name="T2">The type of the second handler parameter.</typeparam>
    /// <typeparam name="T3">The type of the third handler parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth handler parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth handler parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth handler parameter.</typeparam>
    /// <typeparam name="T7">The type of the seventh handler parameter.</typeparam>
    /// <typeparam name="T8">The type of the eighth handler parameter.</typeparam>
    /// <param name="application">The Lambda application.</param>
    /// <param name="handler">An asynchronous handler function accepting eight service parameters.</param>
    /// <returns>The current <see cref="ILambdaApplication" /> instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if called at runtime; this exception is unreachable as this method is intercepted by the
    ///     source generator code at compile time.
    /// </exception>
    public static ILambdaApplication OnShutdown<T1, T2, T3, T4, T5, T6, T7, T8>(
        this ILambdaApplication application,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    /// <summary>
    ///     Registers a shutdown handler that will be run when the Lambda runtime shuts down.
    /// </summary>
    /// <remarks>
    ///     Source generation creates the wiring code to resolve handler dependencies, using compile-time
    ///     interceptors to replace the calls. Dependencies are scoped per handler. If a CancellationToken
    ///     is requested, it will be cancelled before the Lambda runtime forces shutdown.
    /// </remarks>
    /// <typeparam name="T1">The type of the first handler parameter.</typeparam>
    /// <typeparam name="T2">The type of the second handler parameter.</typeparam>
    /// <typeparam name="T3">The type of the third handler parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth handler parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth handler parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth handler parameter.</typeparam>
    /// <typeparam name="T7">The type of the seventh handler parameter.</typeparam>
    /// <typeparam name="T8">The type of the eighth handler parameter.</typeparam>
    /// <typeparam name="T9">The type of the ninth handler parameter.</typeparam>
    /// <param name="application">The Lambda application.</param>
    /// <param name="handler">An asynchronous handler function accepting nine service parameters.</param>
    /// <returns>The current <see cref="ILambdaApplication" /> instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if called at runtime; this exception is unreachable as this method is intercepted by the
    ///     source generator code at compile time.
    /// </exception>
    public static ILambdaApplication OnShutdown<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        this ILambdaApplication application,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    /// <summary>
    ///     Registers a shutdown handler that will be run when the Lambda runtime shuts down.
    /// </summary>
    /// <remarks>
    ///     Source generation creates the wiring code to resolve handler dependencies, using compile-time
    ///     interceptors to replace the calls. Dependencies are scoped per handler. If a CancellationToken
    ///     is requested, it will be cancelled before the Lambda runtime forces shutdown.
    /// </remarks>
    /// <typeparam name="T1">The type of the first handler parameter.</typeparam>
    /// <typeparam name="T2">The type of the second handler parameter.</typeparam>
    /// <typeparam name="T3">The type of the third handler parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth handler parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth handler parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth handler parameter.</typeparam>
    /// <typeparam name="T7">The type of the seventh handler parameter.</typeparam>
    /// <typeparam name="T8">The type of the eighth handler parameter.</typeparam>
    /// <typeparam name="T9">The type of the ninth handler parameter.</typeparam>
    /// <typeparam name="T10">The type of the tenth handler parameter.</typeparam>
    /// <param name="application">The Lambda application.</param>
    /// <param name="handler">An asynchronous handler function accepting ten service parameters.</param>
    /// <returns>The current <see cref="ILambdaApplication" /> instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if called at runtime; this exception is unreachable as this method is intercepted by the
    ///     source generator code at compile time.
    /// </exception>
    public static ILambdaApplication OnShutdown<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        this ILambdaApplication application,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }
}

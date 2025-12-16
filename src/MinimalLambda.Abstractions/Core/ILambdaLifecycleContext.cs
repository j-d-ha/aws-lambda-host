namespace MinimalLambda;

/// <summary>
///     Encapsulates the information available during Lambda lifecycle events such as
///     initialization and shutdown.
/// </summary>
public interface ILambdaLifecycleContext
{
    /// <summary>
    ///     Gets the <see cref="CancellationToken" /> that signals the Lambda lifecycle event is being
    ///     canceled.
    /// </summary>
    /// <remarks>
    ///     The cancellation token will also be cancelled if a SIGTERM signal is received, indicting
    ///     that the Lambda runtime is being terminated.
    /// </remarks>
    CancellationToken CancellationToken { get; }

    /// <summary>Gets or sets a key/value collection that can be used to share data between handlers.</summary>
    IDictionary<string, object?> Properties { get; }

    /// <summary>
    ///     Gets or sets the <see cref="IServiceProvider" /> that provides access to the invocation's
    ///     service container.
    /// </summary>
    IServiceProvider ServiceProvider { get; }

    /// <summary>Gets the elapsed time since the Lambda execution environment was started.</summary>
    TimeSpan ElapsedTime { get; }

    /// <summary>Gets the AWS region where the Lambda function is running, or <c>null</c> if not set.</summary>
    /// <remarks>
    ///     This value is read from the <c>AWS_REGION</c> environment variable. If both
    ///     <c>AWS_REGION</c> and <c>AWS_DEFAULT_REGION</c> are set, <c>AWS_REGION</c> takes precedence.
    /// </remarks>
    string? Region { get; }

    /// <summary>Gets the runtime identifier, or <c>null</c> if not set.</summary>
    /// <remarks>
    ///     This value is read from the <c>AWS_EXECUTION_ENV</c> environment variable, which is
    ///     prefixed by <c>AWS_Lambda_</c> (for example, <c>AWS_Lambda_dotnet8</c>). This environment
    ///     variable is not defined for OS-only runtimes.
    /// </remarks>
    string? ExecutionEnvironment { get; }

    /// <summary>Gets the name of the Lambda function, or <c>null</c> if not set.</summary>
    /// <remarks>
    ///     This value is read from the <c>AWS_LAMBDA_FUNCTION_NAME</c> environment variable set by
    ///     AWS Lambda at cold start.
    /// </remarks>
    string? FunctionName { get; }

    /// <summary>
    ///     Gets the amount of memory available to the function in MB, or <c>null</c> if not set or
    ///     cannot be parsed.
    /// </summary>
    /// <remarks>
    ///     This value is read from the <c>AWS_LAMBDA_FUNCTION_MEMORY_SIZE</c> environment variable
    ///     set by AWS Lambda at cold start.
    /// </remarks>
    int? FunctionMemorySize { get; }

    /// <summary>Gets the version of the function being executed, or <c>null</c> if not set.</summary>
    /// <remarks>
    ///     This value is read from the <c>AWS_LAMBDA_FUNCTION_VERSION</c> environment variable set by
    ///     AWS Lambda at cold start.
    /// </remarks>
    string? FunctionVersion { get; }

    /// <summary>Gets the initialization type of the function, or <c>null</c> if not set.</summary>
    /// <remarks>
    ///     This value is read from the <c>AWS_LAMBDA_INITIALIZATION_TYPE</c> environment variable.
    ///     Possible values include: <c>on-demand</c>, <c>provisioned-concurrency</c>, <c>snap-start</c>,
    ///     or <c>lambda-managed-instances</c>.
    /// </remarks>
    string? InitializationType { get; }

    /// <summary>
    ///     Gets the name of the Amazon CloudWatch Logs group for the function, or <c>null</c> if not
    ///     set.
    /// </summary>
    /// <remarks>
    ///     This value is read from the <c>AWS_LAMBDA_LOG_GROUP_NAME</c> environment variable set by
    ///     AWS Lambda at cold start. This environment variable is not available in Lambda SnapStart
    ///     functions.
    /// </remarks>
    string? LogGroupName { get; }

    /// <summary>
    ///     Gets the name of the Amazon CloudWatch Logs stream for the function, or <c>null</c> if not
    ///     set.
    /// </summary>
    /// <remarks>
    ///     This value is read from the <c>AWS_LAMBDA_LOG_STREAM_NAME</c> environment variable set by
    ///     AWS Lambda at cold start. This environment variable is not available in Lambda SnapStart
    ///     functions.
    /// </remarks>
    string? LogStreamName { get; }

    /// <summary>Gets the path to the Lambda function code, or <c>null</c> if not set.</summary>
    /// <remarks>
    ///     This value is read from the <c>LAMBDA_TASK_ROOT</c> environment variable set by AWS Lambda
    ///     at cold start.
    /// </remarks>
    string? TaskRoot { get; }
}

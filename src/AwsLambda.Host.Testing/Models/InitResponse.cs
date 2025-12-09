namespace AwsLambda.Host.Testing;

/// <summary>
/// Represents the result of a Lambda function initialization attempt.
/// </summary>
public class InitResponse
{
    /// <summary>
    /// Gets the error information if initialization failed, or null if initialization succeeded.
    /// </summary>
    public ErrorResponse? Error { get; internal init; }

    /// <summary>
    /// Gets the status of the initialization attempt.
    /// </summary>
    public InitStatus InitStatus { get; internal init; }
}

/// <summary>
/// An enumeration of possible statuses for Lambda initialization.
/// </summary>
public enum InitStatus
{
    /// <summary>
    /// Initialization of the Lambda completed successfully.
    /// </summary>
    InitCompleted,

    /// <summary>
    /// Initialization of the Lambda failed, and the Lambda returned an error.
    /// </summary>
    InitError,

    /// <summary>
    /// Initialization of the Lambda failed, and the Host process exited.
    /// </summary>
    HostExited,
}

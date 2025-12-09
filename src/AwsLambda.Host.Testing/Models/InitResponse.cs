namespace AwsLambda.Host.Testing;

public class InitResponse
{
    public ErrorResponse? Error { get; internal set; }
    public InitStatus InitStatus { get; internal set; }
}

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

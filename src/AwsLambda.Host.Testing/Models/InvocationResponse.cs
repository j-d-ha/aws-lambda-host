namespace AwsLambda.Host.Testing;

public class InvocationResponse<TResponse>
{
    public bool WasSuccess { get; internal set; }
    public TResponse? Response { get; internal set; }
    public ErrorResponse? Error { get; internal set; }
}

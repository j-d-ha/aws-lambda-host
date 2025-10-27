namespace AwsLambda.Host.Example.OpenTelemetry;

public interface IService
{
    Task<string> GetMessage(string name, CancellationToken cancellationToken = default);
}

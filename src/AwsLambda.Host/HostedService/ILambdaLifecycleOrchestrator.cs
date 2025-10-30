namespace AwsLambda.Host;

internal interface ILambdaLifecycleOrchestrator
{
    Task OnShutdown(List<Exception> exceptions, CancellationToken cancellationToken);
}

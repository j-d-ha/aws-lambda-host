using Amazon.Lambda.Core;

namespace Lambda.Host.Interfaces;

public interface ILambdaCancellationTokenSourceFactory
{
    public CancellationTokenSource NewCancellationTokenSource(ILambdaContext context);
}

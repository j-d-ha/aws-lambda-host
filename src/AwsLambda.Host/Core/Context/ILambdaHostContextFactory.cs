using Amazon.Lambda.Core;

namespace AwsLambda.Host.Core;

internal interface ILambdaHostContextFactory
{
    ILambdaHostContext Create(
        ILambdaContext lambdaContext,
        IDictionary<string, object?> properties,
        CancellationToken cancellationToken
    );
}

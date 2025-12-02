#region

using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;

#endregion

namespace AwsLambda.Host.Core;

internal class LambdaHostContextFactory : ILambdaHostContextFactory
{
    private readonly ILambdaHostContextAccessor? _contextAccessor;
    private readonly IFeatureCollectionFactory _featureCollectionFactory;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private IEnumerable<IFeatureProvider>? _featureProviders;

    public LambdaHostContextFactory(
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollectionFactory featureCollectionFactory,
        ILambdaHostContextAccessor? contextAccessor = null
    )
    {
        ArgumentNullException.ThrowIfNull(serviceScopeFactory);
        ArgumentNullException.ThrowIfNull(featureCollectionFactory);

        _serviceScopeFactory = serviceScopeFactory;
        _contextAccessor = contextAccessor;
        _featureCollectionFactory = featureCollectionFactory;
    }

    public ILambdaHostContext Create(
        ILambdaContext lambdaContext,
        IDictionary<string, object?> properties,
        CancellationToken cancellationToken
    )
    {
        _featureProviders ??=
            properties.TryGetValue(LambdaInvocationBuilder.FeatureProvidersKey, out var value)
            && value is IEnumerable<IFeatureProvider> providers
                ? providers
                : [];

        var context = new DefaultLambdaHostContext(
            lambdaContext,
            _serviceScopeFactory,
            properties,
            _featureCollectionFactory.Create(_featureProviders),
            cancellationToken
        );

        _contextAccessor?.LambdaHostContext = context;

        return context;
    }
}

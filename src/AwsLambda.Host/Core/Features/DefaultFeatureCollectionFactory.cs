namespace AwsLambda.Host.Core.Features;

internal class DefaultFeatureCollectionFactory(IEnumerable<IFeatureProvider> featureProviders)
    : IFeatureCollectionFactory
{
    public IFeatureCollection Create() => new DefaultFeatureCollection(featureProviders);
}

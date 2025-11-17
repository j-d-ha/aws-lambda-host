namespace AwsLambda.Host;

public interface IFeatureCollection : IEnumerable<KeyValuePair<Type, object>>
{
    void Set<T>(T instance);

    T? Get<T>();
}

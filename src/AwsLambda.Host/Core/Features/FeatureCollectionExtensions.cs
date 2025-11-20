using System.Diagnostics.CodeAnalysis;

namespace AwsLambda.Host;

/// <summary>Extension methods for feature collections.</summary>
public static class FeatureCollectionExtensions
{
    extension(IFeatureCollection featureCollection)
    {
        /// <summary>Attempts to get a feature from the collection.</summary>
        /// <typeparam name="T">The type of feature to retrieve.</typeparam>
        /// <param name="result">The retrieved feature, or null if not found.</param>
        /// <returns>True if the feature was found; otherwise false.</returns>
        public bool TryGet<T>([NotNullWhen(true)] out T? result)
        {
            result = featureCollection.Get<T>();
            return result is not null;
        }
    }
}

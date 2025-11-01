namespace AwsLambda.Host.SourceGenerators.Models;

internal readonly record struct HigherOrderMethodInfo(
    DelegateInfo DelegateInfo,
    LocationInfo? LocationInfo,
    InterceptableLocationInfo InterceptableLocationInfo
);

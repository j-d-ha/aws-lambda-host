namespace AwsLambda.Host.SourceGenerators.Models;

internal readonly record struct UseOpenTelemetryTracingInfo(
    LocationInfo? LocationInfo,
    InterceptableLocationInfo InterceptableLocationInfo
);

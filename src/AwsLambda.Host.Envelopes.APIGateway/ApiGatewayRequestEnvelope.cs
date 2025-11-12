using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;

namespace AwsLambda.Host.APIGatewayEnvelops;

/// <inheritdoc cref="Amazon.Lambda.APIGatewayEvents.APIGatewayProxyRequest" />
public class ApiGatewayRequestEnvelope<T> : APIGatewayProxyRequest, IJsonSerializable
{
    /// <summary>The HTTP request body.</summary>
    public new required T? Body { get; set; }

    /// <inheritdoc />
    public static void RegisterTypeInfo(IList<JsonConverter> converters) =>
        converters.Add(new ApiGatewayRequestEnvelopeJsonConverter<ApiGatewayRequestEnvelope<T>>());
}

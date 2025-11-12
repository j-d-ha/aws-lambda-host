using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;

namespace AwsLambda.Host.Envelopes.APIGateway;

/// <inheritdoc cref="Amazon.Lambda.APIGatewayEvents.APIGatewayProxyResponse" />
public class ApiGatewayResponseEnvelope<T> : APIGatewayProxyResponse, IJsonSerializable
{
    /// <summary>The response body</summary>
    public new required T? Body { get; set; }

    /// <inheritdoc />
    public static void RegisterTypeInfo(IList<JsonConverter> converters) =>
        converters.Add(new ApiGatewayResponseJsonConverter<ApiGatewayResponseEnvelope<T>>());
}

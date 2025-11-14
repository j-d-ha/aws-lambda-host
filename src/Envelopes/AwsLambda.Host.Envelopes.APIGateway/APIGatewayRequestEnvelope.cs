using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;

namespace AwsLambda.Host.Envelopes.APIGateway;

/// <inheritdoc cref="Amazon.Lambda.APIGatewayEvents.APIGatewayProxyRequest" />
public class APIGatewayRequestEnvelope<T> : APIGatewayProxyRequest, IEnvelope
{
    /// <summary>The deserialized content of the HTTP request body.</summary>
    [JsonIgnore]
    public new T? Body { get; set; }

    public void ExtractPayload(JsonSerializerOptions options) =>
        Body = JsonSerializer.Deserialize<T>(((APIGatewayProxyRequest)this).Body, options);

    public void PackPayload(JsonSerializerOptions options) =>
        ((APIGatewayProxyRequest)this).Body = JsonSerializer.Serialize(Body, options);
}

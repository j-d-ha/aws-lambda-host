using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;

namespace AwsLambda.Host.Envelopes.APIGateway;

/// <inheritdoc cref="Amazon.Lambda.APIGatewayEvents.APIGatewayProxyResponse" />
public class APIGatewayResponseEnvelope<T> : APIGatewayProxyResponse, IEnvelope
{
    /// <summary>The content of the response body</summary>
    [JsonIgnore]
    public new T? Body { get; set; }

    public void ExtractPayload(JsonSerializerOptions options) =>
        Body = JsonSerializer.Deserialize<T>(((APIGatewayProxyResponse)this).Body, options);

    public void PackPayload(JsonSerializerOptions options) =>
        ((APIGatewayProxyResponse)this).Body = JsonSerializer.Serialize(Body, options);
}

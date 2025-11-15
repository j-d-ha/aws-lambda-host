using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using AwsLambda.Host.Options;

namespace AwsLambda.Host.Envelopes.ApiGateway;

/// <inheritdoc cref="Amazon.Lambda.APIGatewayEvents.APIGatewayHttpApiV2ProxyResponse" />
/// <remarks>
///     This class extends
///     <see cref="Amazon.Lambda.APIGatewayEvents.APIGatewayHttpApiV2ProxyResponse" /> and adds a
///     strongly typed <see cref="BodyContent" /> property for easier serialization and deserialization
///     of response payloads.
/// </remarks>
public class ApiGatewayV2ResponseEnvelope<T> : APIGatewayHttpApiV2ProxyResponse, IResponseEnvelope
{
    /// <summary>The unserialized content of the <see cref="APIGatewayProxyResponse.Body" /></summary>
    [JsonIgnore]
    public T? BodyContent { get; set; }

    /// <inheritdoc />
    public void PackPayload(EnvelopeOptions options) =>
        Body = JsonSerializer.Serialize(BodyContent, options.JsonOptions);
}

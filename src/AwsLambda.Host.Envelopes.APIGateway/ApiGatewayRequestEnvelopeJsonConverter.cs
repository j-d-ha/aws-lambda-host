using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;

namespace AwsLambda.Host.APIGatewayEnvelops;

/// <inheritdoc />
public class ApiGatewayRequestEnvelopeJsonConverter<T> : JsonConverter<ApiGatewayRequestEnvelope<T>>
{
    /// <inheritdoc />
    public override ApiGatewayRequestEnvelope<T>? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var baseEvent = JsonSerializer.Deserialize<APIGatewayProxyRequest>(ref reader, options);
        if (baseEvent is null)
            return null;

        var outEvent = (ApiGatewayRequestEnvelope<T>)baseEvent;
        outEvent.Body = JsonSerializer.Deserialize<T>(baseEvent.Body, options);

        return outEvent;
    }

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer,
        ApiGatewayRequestEnvelope<T> value,
        JsonSerializerOptions options
    )
    {
        var body = JsonSerializer.Serialize(value.Body, options);
        APIGatewayProxyRequest outEvent = value;
        outEvent.Body = body;
        JsonSerializer.Serialize(writer, outEvent, options);
    }
}

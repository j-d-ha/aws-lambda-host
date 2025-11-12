using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;

namespace AwsLambda.Host.Envelopes.APIGateway;

/// <inheritdoc />
public class ApiGatewayResponseJsonConverter<T> : JsonConverter<ApiGatewayResponseEnvelope<T>>
{
    /// <inheritdoc />
    public override ApiGatewayResponseEnvelope<T>? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var baseEvent = JsonSerializer.Deserialize<APIGatewayProxyResponse>(ref reader, options);
        if (baseEvent is null)
            return null;

        var outEvent = (ApiGatewayResponseEnvelope<T>)baseEvent;
        outEvent.Body = JsonSerializer.Deserialize<T>(baseEvent.Body, options);

        return outEvent;
    }

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer,
        ApiGatewayResponseEnvelope<T> value,
        JsonSerializerOptions options
    )
    {
        var body = JsonSerializer.Serialize(value.Body, options);
        APIGatewayProxyResponse outEvent = value;
        outEvent.Body = body;
        JsonSerializer.Serialize(writer, outEvent, options);
    }
}

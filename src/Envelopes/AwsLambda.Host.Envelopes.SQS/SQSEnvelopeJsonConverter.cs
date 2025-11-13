using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.SQSEvents;

namespace AwsLambda.Host.Envelopes.SQS;

/// <summary>JSON converter for AWS SQS envelope with typed body payloads.</summary>
/// <typeparam name="T">The type of the deserialized body payload.</typeparam>
/// <remarks>
///     Handles serialization and deserialization of <see cref="SQSEnvelope{T}" /> instances,
///     converting the body string in each record to and from the specified type
///     <typeparamref name="T" />.
/// </remarks>
public class SQSEnvelopeJsonConverter<T> : EnvelopeJsonConverter<SQSEnvelope<T>>
{
    /// <inheritdoc />
    protected override void ReadPayload(SQSEnvelope<T> value, JsonSerializerOptions options)
    {
        foreach (var record in value.Records)
            record.Body = JsonSerializer.Deserialize<T>(
                ((SQSEvent.SQSMessage)record).Body,
                options
            );
    }

    /// <inheritdoc />
    protected override void WritePayload(SQSEnvelope<T> value, JsonSerializerOptions options)
    {
        foreach (var record in value.Records)
            ((SQSEvent.SQSMessage)record).Body = JsonSerializer.Serialize(record.Body, options);
    }

    /// <inheritdoc />
    protected override JsonConverter GetConverterInstance() => this;
}

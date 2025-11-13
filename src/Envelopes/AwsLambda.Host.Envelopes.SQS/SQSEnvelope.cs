using System.Text.Json.Serialization;
using Amazon.Lambda.SQSEvents;

namespace AwsLambda.Host.Envelopes.SQS;

/// <inheritdoc cref="SQSEvent" />
public class SQSEnvelope<T> : SQSEvent, IJsonSerializable
{
    /// <summary>Get and sets the Records</summary>
    public new required List<SQSMessageEnvelope> Records { get; set; }

    /// <inheritdoc />
    public static void RegisterConverter(IList<JsonConverter> converters) =>
        converters.Add(new SQSEnvelopeJsonConverter<T>());

    /// <inheritdoc />
    public class SQSMessageEnvelope : SQSMessage
    {
        /// <summary>Get and sets the Body</summary>
        [JsonIgnore]
        public new required T? Body { get; set; }
    }
}

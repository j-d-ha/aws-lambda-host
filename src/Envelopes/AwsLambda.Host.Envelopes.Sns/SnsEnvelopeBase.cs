using System.Text.Json.Serialization;
using Amazon.Lambda.SNSEvents;
using AwsLambda.Host.Options;

namespace AwsLambda.Host.Envelopes.Sns;

/// <inheritdoc cref="SNSEvent" />
/// <remarks>
///     This abstract class extends <see cref="SNSEvent" /> and provides a foundation for strongly
///     typed SNS message handling. Derived classes implement <see cref="ExtractPayload" /> to
///     deserialize the message bodies into strongly typed <see cref="SnsMessageEnvelope" /> records
///     using their chosen deserialization strategy.
/// </remarks>
public abstract class SnsEnvelopeBase<T> : SNSEvent, IRequestEnvelope
{
    /// <inheritdoc cref="SNSEvent.Records" />
    public new required List<SnsMessageEnvelope> Records { get; set; }

    /// <inheritdoc />
    public abstract void ExtractPayload(EnvelopeOptions options);

    /// <inheritdoc cref="SNSEvent.SNSRecord" />
    public class SnsMessageEnvelope : SNSRecord
    {
        /// <summary>Gets and sets the deserialized <see cref="SNSEvent.SNSRecord.Sns" /> message body</summary>
        [JsonIgnore]
        public T? BodyContent { get; set; }
    }
}

using Amazon.Lambda.KinesisFirehoseEvents;
using AwsLambda.Host.Options;

namespace AwsLambda.Host.Envelopes.KinesisFirehose;

public abstract class KinesisFirehoseEventEnvelopeBase<T> : KinesisFirehoseEvent, IRequestEnvelope
{
    public new required IList<FirehoseRecordEnvelope> Records { get; set; }

    /// <inheritdoc />
    public abstract void ExtractPayload(EnvelopeOptions options);

    public class FirehoseRecordEnvelope : FirehoseRecord
    {
        public T? DataContent { get; set; }
    }
}

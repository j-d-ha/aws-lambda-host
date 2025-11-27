using Amazon.Lambda.KinesisFirehoseEvents;
using AwsLambda.Host.Options;

namespace AwsLambda.Host.Envelopes.KinesisFirehose;

public abstract class KinesisFirehoseResponseEnvelopeBase<T>
    : KinesisFirehoseResponse,
        IResponseEnvelope
{
    public new required IList<FirehoseRecordEnvelope> Records { get; set; }

    public abstract void PackPayload(EnvelopeOptions options);

    public class FirehoseRecordEnvelope : FirehoseRecord
    {
        public T? DataContent { get; set; }
    }
}

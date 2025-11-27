using Amazon.Lambda.KinesisEvents;
using AwsLambda.Host.Options;

namespace AwsLambda.Host.Envelopes.Kinesis;

public abstract class KinesisEnvelopeBase<T> : KinesisEvent, IRequestEnvelope
{
    public new required IList<KinesisEventRecordEnvelope> Records { get; set; }

    public abstract void ExtractPayload(EnvelopeOptions options);

    public class KinesisEventRecordEnvelope : KinesisEventRecord
    {
        public new required RecordEnvelope Kinesis { get; set; }
    }

    public class RecordEnvelope : Record
    {
        public T? DataContent { get; set; }
    }
}

using System.Text.Json;
using AwsLambda.Host.Options;

namespace AwsLambda.Host.Envelopes.KinesisFirehose;

public sealed class KinesisFirehoseResponseEnvelope<T> : KinesisFirehoseResponseEnvelopeBase<T>
{
    public override void PackPayload(EnvelopeOptions options)
    {
        foreach (var record in Records)
        {
            var serializedData = JsonSerializer.Serialize(record.DataContent, options.JsonOptions);
            record.EncodeData(serializedData);
        }
    }
}

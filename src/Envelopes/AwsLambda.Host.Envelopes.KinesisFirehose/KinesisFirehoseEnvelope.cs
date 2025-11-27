using System.Text.Json;
using AwsLambda.Host.Options;

namespace AwsLambda.Host.Envelopes.KinesisFirehose;

public sealed class KinesisFirehoseEnvelope<T> : KinesisFirehoseEnvelopeBase<T>
{
    public override void ExtractPayload(EnvelopeOptions options)
    {
        foreach (var record in Records)
            record.DataContent = JsonSerializer.Deserialize<T>(
                record.DecodeData(),
                options.JsonOptions
            );
    }
}

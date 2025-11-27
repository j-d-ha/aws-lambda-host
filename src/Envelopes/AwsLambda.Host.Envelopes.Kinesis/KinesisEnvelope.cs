using System.Text;
using System.Text.Json;
using AwsLambda.Host.Options;

namespace AwsLambda.Host.Envelopes.Kinesis;

public class KinesisEnvelope<T> : KinesisEnvelopeBase<T>
{
    public override void ExtractPayload(EnvelopeOptions options)
    {
        foreach (var record in Records)
        {
            using var reader = new StreamReader(
                record.Kinesis.Data,
                Encoding.UTF8,
                leaveOpen: true
            );
            var base64String = reader.ReadToEnd();
            var jsonBytes = Convert.FromBase64String(base64String);
            record.Kinesis.DataContent = JsonSerializer.Deserialize<T>(
                jsonBytes,
                options.JsonOptions
            );
        }
    }
}

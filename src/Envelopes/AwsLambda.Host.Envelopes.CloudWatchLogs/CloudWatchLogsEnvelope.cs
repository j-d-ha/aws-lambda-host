using System.Text.Json;
using AwsLambda.Host.Options;

namespace AwsLambda.Host.Envelopes.CloudWatchLogs;

public sealed class CloudWatchLogsEnvelope<T> : CloudWatchLogsEnvelopeBase<T>
{
    public override void ExtractPayload(EnvelopeOptions options) =>
        Awslogs.DataContent = JsonSerializer.Deserialize<T>(
            Awslogs.DecodeData(),
            options.JsonOptions
        );
}

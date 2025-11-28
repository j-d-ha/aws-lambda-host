using Amazon.Lambda.CloudWatchLogsEvents;
using AwsLambda.Host.Options;

namespace AwsLambda.Host.Envelopes.CloudWatchLogs;

public abstract class CloudWatchLogsEnvelopeBase<T> : CloudWatchLogsEvent, IRequestEnvelope
{
    public new required LogEnvelope Awslogs { get; set; }

    public abstract void ExtractPayload(EnvelopeOptions options);

    public class LogEnvelope : Log
    {
        public T? DataContent { get; set; }
    }
}

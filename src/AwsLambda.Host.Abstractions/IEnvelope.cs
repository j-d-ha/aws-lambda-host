using System.Text.Json;

namespace AwsLambda.Host;

public interface IEnvelope
{
    void ExtractPayload(JsonSerializerOptions options);

    void PackPayload(JsonSerializerOptions options);
}

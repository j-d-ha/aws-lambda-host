using System.Text.Json;

namespace AwsLambda.Host.Options;

public class EnvelopeOptions
{
    public JsonSerializerOptions JsonOptions { get; set; } = new();
}

using System.Text.Json;
using Amazon.Lambda.Core;

namespace AwsLambda.Host;

public interface ILambdaResponse<in TSelf>
    where TSelf : ILambdaResponse<TSelf>
{
    void Serialize(
        ILambdaSerializer lambdaSerializer,
        Stream responseStream,
        JsonSerializerOptions? jsonSerializerOptions
    );
}

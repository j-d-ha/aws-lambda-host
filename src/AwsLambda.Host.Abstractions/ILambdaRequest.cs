using System.Text.Json;
using Amazon.Lambda.Core;

namespace AwsLambda.Host;

public interface ILambdaRequest<out TSelf>
    where TSelf : ILambdaRequest<TSelf>
{
    static abstract TSelf Deserialize(
        Stream requestStream,
        ILambdaSerializer lambdaSerializer,
        JsonSerializerOptions? jsonSerializerOptions
    );
}

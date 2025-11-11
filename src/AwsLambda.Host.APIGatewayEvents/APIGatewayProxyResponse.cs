using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace AwsLambda.Host.APIGatewayEvents;

/// <inheritdoc cref="Amazon.Lambda.APIGatewayEvents.APIGatewayProxyResponse" />
public class APIGatewayProxyResponse<T>
    : APIGatewayProxyResponse,
        ILambdaResponse<APIGatewayProxyResponse<T>>
{
    /// <summary>The response body</summary>
    public new required T Body { get; set; }

    public void Serialize(
        ILambdaSerializer serializer,
        Stream stream,
        JsonSerializerOptions? jsonSerializerOptions
    )
    {
        var body = JsonSerializer.Serialize(Body, jsonSerializerOptions);

        var outResponse = new APIGatewayProxyResponse
        {
            StatusCode = StatusCode,
            Headers = Headers,
            MultiValueHeaders = MultiValueHeaders,
            Body = body,
            IsBase64Encoded = IsBase64Encoded,
        };

        serializer.Serialize(outResponse, stream);
    }
}

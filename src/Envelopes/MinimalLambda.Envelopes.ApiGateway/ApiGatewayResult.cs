using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using MinimalLambda.Options;

namespace MinimalLambda.Envelopes.ApiGateway;

public sealed class ApiGatewayResult : APIGatewayProxyResponse, IHttpResult<ApiGatewayResult>
{
    [JsonIgnore]
    private readonly IResponseEnvelope? _inner;

    private ApiGatewayResult(APIGatewayProxyResponse response)
    {
        _inner = response as IResponseEnvelope;
        base.StatusCode = response.StatusCode;
        Headers = response.Headers;
        MultiValueHeaders = response.MultiValueHeaders;
        Body = response.Body;
        IsBase64Encoded = response.IsBase64Encoded;
    }

    /// <inheritdoc />
    public void PackPayload(EnvelopeOptions options)
    {
        if (_inner is null)
            return;

        _inner.PackPayload(options);
        Body = ((APIGatewayProxyResponse)_inner).Body;
    }

    //      ┌──────────────────────────────────────────────────────────┐
    //      │                     Headers Helpers                      │
    //      └──────────────────────────────────────────────────────────┘

    public ApiGatewayResult AddHeader(string key, string value)
    {
        Headers[key] = value;

        return this;
    }

    public ApiGatewayResult AddContentType(string contentType) =>
        AddHeader("Content-Type", contentType);

    //      ┌──────────────────────────────────────────────────────────┐
    //      │                      Basic Fatories                      │
    //      └──────────────────────────────────────────────────────────┘

    public static ApiGatewayResult Create<T>(
        int statusCode,
        T? bodyContent,
        string? body,
        IDictionary<string, string>? headers,
        bool isBase64Encoded
    ) =>
        new(
            new ApiGatewayResponseEnvelope<T>
            {
                StatusCode = statusCode,
                BodyContent = bodyContent,
                Body = body ?? string.Empty,
                Headers = headers,
                IsBase64Encoded = isBase64Encoded,
            }
        );

    // public static ApiGatewayResult Create<T>(ApiGatewayResponseEnvelopeBase<T> response) =>
    //     new(response);
    //
    // public static ApiGatewayResult Create<T>(
    //     int statusCode,
    //     T? bodyContent,
    //     Dictionary<string, string> headers,
    //     IDictionary<string, IList<string>> multiValueHeaders
    // ) =>
    //     Create(
    //         new ApiGatewayResponseEnvelope<T>
    //         {
    //             BodyContent = bodyContent,
    //             StatusCode = statusCode,
    //             Headers = headers,
    //             MultiValueHeaders = multiValueHeaders,
    //         }
    //     );
    //
    // public static ApiGatewayResult Create<T>(
    //     int statusCode,
    //     string body,
    //     Dictionary<string, string> headers,
    //     IDictionary<string, IList<string>> multiValueHeaders
    // ) =>
    //     Create(
    //         new APIGatewayProxyResponse
    //         {
    //             StatusCode = statusCode,
    //             Headers = headers,
    //             MultiValueHeaders = multiValueHeaders,
    //             Body = body,
    //         }
    //     );
    //
    // public static ApiGatewayResult Json<T>(int statusCode, T bodyContent) =>
    //     Create(
    //         new ApiGatewayResponseEnvelope<T>
    //         {
    //             BodyContent = bodyContent,
    //             StatusCode = statusCode,
    //             Headers = new Dictionary<string, string>
    //             {
    //                 ["Content-Type"] = "application/json; charset=utf-8",
    //             },
    //         }
    //     );
    //
    // public static ApiGatewayResult Text(int statusCode, string body) =>
    //     Create(
    //         new APIGatewayProxyResponse
    //         {
    //             StatusCode = statusCode,
    //             Headers = new Dictionary<string, string>
    //             {
    //                 ["Content-Type"] = "text/plain; charset=utf-8",
    //             },
    //             Body = body,
    //         }
    //     );
    //
    // public static ApiGatewayResult StatusCode(int statusCode) =>
    //     Create(new APIGatewayProxyResponse { StatusCode = statusCode });
    //
    // //      ┌──────────────────────────────────────────────────────────┐
    // //      │                  StatusCode Code Factories               │
    // //      └──────────────────────────────────────────────────────────┘
    //
}

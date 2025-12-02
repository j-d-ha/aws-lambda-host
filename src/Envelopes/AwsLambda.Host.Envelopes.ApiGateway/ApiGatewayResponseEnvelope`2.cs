#region

using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using AwsLambda.Host.Options;

#endregion

namespace AwsLambda.Host.Envelopes.ApiGateway;

public abstract class ApiGatewayResult : APIGatewayProxyResponse, IResponseEnvelope
{
    [JsonIgnore]
    private readonly IResponseEnvelope? _inner;

    protected ApiGatewayResult() { }

    protected ApiGatewayResult(APIGatewayProxyResponse response)
    {
        _inner = (IResponseEnvelope)response;
        StatusCode = response.StatusCode;
        Headers = response.Headers;
        MultiValueHeaders = response.MultiValueHeaders;
        Body = response.Body;
        IsBase64Encoded = response.IsBase64Encoded;
    }

    public void PackPayload(EnvelopeOptions options)
    {
        if (_inner is null)
            return;

        _inner.PackPayload(options);
        Body = ((APIGatewayProxyResponse)_inner).Body;
    }
}

public sealed class ApiGatewayResult<T1, T2> : ApiGatewayResult
{
    private ApiGatewayResult(APIGatewayProxyResponse response)
        : base(response) { }

    public static implicit operator ApiGatewayResult<T1, T2>(
        ApiGatewayResponseEnvelopeBase<T1> response
    ) => new(response);

    public static implicit operator ApiGatewayResult<T1, T2>(
        ApiGatewayResponseEnvelopeBase<T2> response
    ) => new(response);
}

public sealed class ApiGatewayResult<T1, T2, T3> : ApiGatewayResult
{
    private ApiGatewayResult(APIGatewayProxyResponse response)
        : base(response) { }

    public static implicit operator ApiGatewayResult<T1, T2, T3>(
        ApiGatewayResponseEnvelopeBase<T1> response
    ) => new(response);

    public static implicit operator ApiGatewayResult<T1, T2, T3>(
        ApiGatewayResponseEnvelopeBase<T2> response
    ) => new(response);

    public static implicit operator ApiGatewayResult<T1, T2, T3>(
        ApiGatewayResponseEnvelopeBase<T3> response
    ) => new(response);
}

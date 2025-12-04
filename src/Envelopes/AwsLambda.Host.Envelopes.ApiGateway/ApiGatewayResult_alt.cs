using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using AwsLambda.Host.Options;

namespace AwsLambda.Host.Envelopes.ApiGateway;

public abstract class ApiGatewayResult_alt : APIGatewayProxyResponse, IResponseEnvelope
{
    [JsonIgnore]
    private readonly IResponseEnvelope? _inner;

    protected ApiGatewayResult_alt() { }

    protected ApiGatewayResult_alt(APIGatewayProxyResponse response)
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

public sealed class ApiGatewayResultAlt<T1, T2> : ApiGatewayResult_alt
{
    private ApiGatewayResultAlt(APIGatewayProxyResponse response)
        : base(response) { }

    public static implicit operator ApiGatewayResultAlt<T1, T2>(
        ApiGatewayResponseEnvelopeBase<T1> response
    ) => new(response);

    public static implicit operator ApiGatewayResultAlt<T1, T2>(
        ApiGatewayResponseEnvelopeBase<T2> response
    ) => new(response);
}

public sealed class ApiGatewayResultAlt<T1, T2, T3> : ApiGatewayResult_alt
{
    private ApiGatewayResultAlt(APIGatewayProxyResponse response)
        : base(response) { }

    public static implicit operator ApiGatewayResultAlt<T1, T2, T3>(
        ApiGatewayResponseEnvelopeBase<T1> response
    ) => new(response);

    public static implicit operator ApiGatewayResultAlt<T1, T2, T3>(
        ApiGatewayResponseEnvelopeBase<T2> response
    ) => new(response);

    public static implicit operator ApiGatewayResultAlt<T1, T2, T3>(
        ApiGatewayResponseEnvelopeBase<T3> response
    ) => new(response);
}

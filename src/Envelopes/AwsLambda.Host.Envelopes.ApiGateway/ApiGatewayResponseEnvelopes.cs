#region

using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using AwsLambda.Host.Options;

#endregion

namespace AwsLambda.Host.Envelopes.ApiGateway;

public class ApiGatewayResponseEnvelopes<T1, T2> : APIGatewayProxyResponse, IResponseEnvelope
{
    [JsonIgnore]
    private IResponseEnvelope? _inner;

    public void PackPayload(EnvelopeOptions options)
    {
        if (_inner is null)
            return;

        _inner.PackPayload(options);
        Body = ((APIGatewayProxyResponse)_inner).Body;
    }

    public static implicit operator ApiGatewayResponseEnvelopes<T1, T2>(
        ApiGatewayResponseEnvelopeBase<T1> response
    ) => CreateFrom(response);

    public static implicit operator ApiGatewayResponseEnvelopes<T1, T2>(
        ApiGatewayResponseEnvelopeBase<T2> response
    ) => CreateFrom(response);

    private static ApiGatewayResponseEnvelopes<T1, T2> CreateFrom(
        APIGatewayProxyResponse response
    ) =>
        new()
        {
            _inner = (IResponseEnvelope)response,
            StatusCode = response.StatusCode,
            Headers = response.Headers,
            MultiValueHeaders = response.MultiValueHeaders,
            Body = response.Body,
            IsBase64Encoded = response.IsBase64Encoded,
        };
}

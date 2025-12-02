#region

using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using AwsLambda.Host.Options;

#endregion

namespace AwsLambda.Host.Envelopes.ApiGateway;

public class ApiGatewayResponseEnvelopes<T1, T2> : APIGatewayProxyResponse, IResponseEnvelope
{
    [JsonIgnore]
    private ApiGatewayResponseEnvelopeBase<T1>? _envelope1;

    [JsonIgnore]
    private ApiGatewayResponseEnvelopeBase<T2>? _envelope2;

    public void PackPayload(EnvelopeOptions options)
    {
        if (_envelope1 is not null)
        {
            _envelope1.PackPayload(options);
            Body = _envelope1.Body;
        }
        else if (_envelope2 is not null)
        {
            _envelope2.PackPayload(options);
            Body = _envelope2.Body;
        }
    }

    public static implicit operator ApiGatewayResponseEnvelopes<T1, T2>(
        ApiGatewayResponseEnvelopeBase<T1> response
    )
    {
        var envelope = Create(response);
        envelope._envelope1 = response;
        return envelope;
    }

    public static implicit operator ApiGatewayResponseEnvelopes<T1, T2>(
        ApiGatewayResponseEnvelopeBase<T2> response
    )
    {
        var envelope = Create(response);
        envelope._envelope2 = response;
        return envelope;
    }

    private static ApiGatewayResponseEnvelopes<T1, T2> Create(APIGatewayProxyResponse response) =>
        new()
        {
            StatusCode = response.StatusCode,
            Headers = response.Headers,
            MultiValueHeaders = response.MultiValueHeaders,
            Body = response.Body,
            IsBase64Encoded = response.IsBase64Encoded,
        };
}

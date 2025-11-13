using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using AutoFixture;
using AwesomeAssertions;
using AwsLambda.Host.Envelopes.APIGateway;
using JetBrains.Annotations;
using Xunit;

namespace AwsLambda.Host.Envelopes.UnitTests;

/// <summary>
///     Unit tests for <see cref="APIGatewayRequestEnvelopeJsonConverter{T}" /> and
///     <see cref="APIGatewayRequestEnvelope{T}" />. Tests verify the envelope converter system which
///     bridges AWS Lambda's string-based request bodies with strongly-typed payloads while maintaining
///     proper isolation of user serialization policies from the AWS envelope structure.
/// </summary>
[TestSubject(typeof(APIGatewayRequestEnvelopeJsonConverter<>))]
[TestSubject(typeof(APIGatewayRequestEnvelope<>))]
public class APIGatewayRequestEnvelopeJsonConverterTests
{
    private readonly Fixture _fixture = new();

    #region Naming Policy Isolation Tests

    [Fact]
    public void Deserialize_WithCamelCasePolicyUser_EnvelopeStructurePreserved()
    {
        // Arrange - User has configured CamelCase naming policy
        // This tests that AWS envelope properties (Path, HttpMethod, etc.) are NOT affected
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        APIGatewayRequestEnvelope<CamelCasePayload>.RegisterConverter(options.Converters);

        var payload = _fixture.Create<CamelCasePayload>();
        // Body must be serialized with camelCase to match the policy
        var bodyJson = JsonSerializer.Serialize(payload, options);
        var request = _fixture.Build<APIGatewayProxyRequest>().With(r => r.Body, bodyJson).Create();

        // AWS would send this JSON (PascalCase for envelope, user controls body)
        var json = JsonSerializer.Serialize(request);

        // Act - Deserialize with user's CamelCase policy
        var envelope = JsonSerializer.Deserialize<APIGatewayRequestEnvelope<CamelCasePayload>>(
            json,
            options
        );

        // Assert - Envelope properties MUST be read correctly despite camelCase policy
        // This is the critical bug prevention: envelope structure is not affected by user policy
        envelope.Should().NotBeNull();
        request
            .Should()
            .BeEquivalentTo(
                envelope,
                options =>
                {
                    // dict of string, object plays weird here
                    options.Excluding(e => e.RequestContext.Authorizer);
                    // Body on envelope is T but will be string on inner type
                    options.Excluding(e => e.Body);
                    return options;
                }
            );
        envelope.Body.Should().BeEquivalentTo(payload);
    }

    #endregion

    #region Property Shadowing and Type Correctness

    [Fact]
    public void Deserialize_BodyPropertyShadowing_TypedBodyReturned()
    {
        // Arrange - Verify that typed Body (T) is returned, not string Body
        var options = new JsonSerializerOptions();
        APIGatewayRequestEnvelope<SimplePayload>.RegisterConverter(options.Converters);

        var payload = _fixture.Create<SimplePayload>();
        var request = _fixture
            .Build<APIGatewayProxyRequest>()
            .With(r => r.Body, JsonSerializer.Serialize(payload))
            .Create();
        var json = JsonSerializer.Serialize(request);

        // Act
        var envelope = JsonSerializer.Deserialize<APIGatewayRequestEnvelope<SimplePayload>>(
            json,
            options
        );

        // Assert - Body is typed as SimplePayload, has the properties
        envelope.Should().NotBeNull();
        envelope!.Body.Should().NotBeNull();
        // These properties only exist on SimplePayload, not on string
        envelope.Body.Name.Should().Be(payload.Name);
        envelope.Body.Value.Should().Be(payload.Value);
    }

    #endregion

    #region Multiple Payload Types

    [Fact]
    public void Deserialize_WithMultiplePayloadTypes_EachHandledIndependently()
    {
        // Arrange - Register multiple payload types
        var options = new JsonSerializerOptions();
        APIGatewayRequestEnvelope<int>.RegisterConverter(options.Converters);
        APIGatewayRequestEnvelope<SimplePayload>.RegisterConverter(options.Converters);

        var intValue = _fixture.Create<int>();
        var payloadValue = _fixture.Create<SimplePayload>();

        var intRequest = _fixture
            .Build<APIGatewayProxyRequest>()
            .With(r => r.Body, JsonSerializer.Serialize(intValue))
            .Create();
        var payloadRequest = _fixture
            .Build<APIGatewayProxyRequest>()
            .With(r => r.Body, JsonSerializer.Serialize(payloadValue))
            .Create();

        var intJson = JsonSerializer.Serialize(intRequest);
        var payloadJson = JsonSerializer.Serialize(payloadRequest);

        // Act
        var intEnvelope = JsonSerializer.Deserialize<APIGatewayRequestEnvelope<int>>(
            intJson,
            options
        );
        var payloadEnvelope = JsonSerializer.Deserialize<APIGatewayRequestEnvelope<SimplePayload>>(
            payloadJson,
            options
        );

        // Assert - Each type is handled with its own converter
        intEnvelope.Should().NotBeNull();
        intEnvelope!.Body.Should().Be(intValue);

        payloadEnvelope.Should().NotBeNull();
        payloadEnvelope!.Body.Should().BeEquivalentTo(payloadValue);
    }

    #endregion

    #region Body Type Conversion Tests

    [Fact]
    public void Deserialize_StringBodyToTypedBody_CorrectlyDeserializes()
    {
        // Arrange - Body field is a JSON string that needs to become strongly-typed
        var options = new JsonSerializerOptions();
        APIGatewayRequestEnvelope<SimplePayload>.RegisterConverter(options.Converters);

        var payload = _fixture.Create<SimplePayload>();
        var request = _fixture
            .Build<APIGatewayProxyRequest>()
            .With(r => r.Body, JsonSerializer.Serialize(payload))
            .Create();
        var json = JsonSerializer.Serialize(request);

        // Act - Deserialize the string body into strongly-typed SimplePayload
        var envelope = JsonSerializer.Deserialize<APIGatewayRequestEnvelope<SimplePayload>>(
            json,
            options
        );

        // Assert - Body is now typed as SimplePayload, not a string
        envelope.Should().NotBeNull();
        envelope!.Body.Should().BeEquivalentTo(payload);
        envelope.Body.Name.Should().Be(payload.Name);
        envelope.Body.Value.Should().Be(payload.Value);
    }

    [Fact]
    public void Serialize_TypedBodyToStringBody_CorrectlySerializes()
    {
        // Arrange - Start with typed body, serialize to API Gateway format
        var options = new JsonSerializerOptions();
        APIGatewayRequestEnvelope<SimplePayload>.RegisterConverter(options.Converters);

        var payload = _fixture.Create<SimplePayload>();
        var envelope = _fixture
            .Build<APIGatewayRequestEnvelope<SimplePayload>>()
            .With(e => e.Body, payload)
            .Create();

        // Act - Serialize the envelope
        var json = JsonSerializer.Serialize(envelope, options);

        // Assert - JSON should contain the body as a serialized string (not double-serialized)
        json.Should().Contain("\"Body\":");

        // Deserialize to verify round-trip
        var deserialized = JsonSerializer.Deserialize<APIGatewayRequestEnvelope<SimplePayload>>(
            json,
            options
        );
        deserialized.Should().NotBeNull();
        deserialized!.Body.Should().BeEquivalentTo(payload);
    }

    #endregion

    #region Edge Cases - Null and Empty Bodies

    [Fact]
    public void Deserialize_WithNullBodyString_ResultsInNullTypedBody()
    {
        // Arrange - Body field contains "null" (valid JSON for null value)
        var options = new JsonSerializerOptions();
        APIGatewayRequestEnvelope<SimplePayload?>.RegisterConverter(options.Converters);

        var request = _fixture.Build<APIGatewayProxyRequest>().With(r => r.Body, "null").Create();
        var json = JsonSerializer.Serialize(request);

        // Act - Deserialize with null body
        var envelope = JsonSerializer.Deserialize<APIGatewayRequestEnvelope<SimplePayload?>>(
            json,
            options
        );

        // Assert - Body should be null, envelope should still exist
        envelope.Should().NotBeNull();
        envelope!.Body.Should().BeNull();
        envelope.Path.Should().Be(request.Path);
        envelope.HttpMethod.Should().Be(request.HttpMethod);
    }

    [Fact]
    public void Deserialize_WithEmptyStringBody_ThrowsJsonException()
    {
        // Arrange - Body field is empty string (invalid JSON)
        var options = new JsonSerializerOptions();
        APIGatewayRequestEnvelope<SimplePayload>.RegisterConverter(options.Converters);

        var request = _fixture.Build<APIGatewayProxyRequest>().With(r => r.Body, "").Create();
        var json = JsonSerializer.Serialize(request);

        // Act & Assert - Should throw because empty string is not valid JSON
        var act = () =>
            JsonSerializer.Deserialize<APIGatewayRequestEnvelope<SimplePayload>>(json, options);
        act.Should().Throw<JsonException>();
    }

    #endregion

    #region Type Mismatch and Error Handling

    [Fact]
    public void Deserialize_WithWrongTypeInBody_DeserializesWithDefaults()
    {
        // Arrange - Body contains valid JSON but wrong shape for SimplePayload
        // JSON deserializer uses default values when properties are missing
        var options = new JsonSerializerOptions();
        APIGatewayRequestEnvelope<SimplePayload>.RegisterConverter(options.Converters);

        var wrongPayload = new { UnexpectedField = "value" };
        var request = _fixture
            .Build<APIGatewayProxyRequest>()
            .With(r => r.Body, JsonSerializer.Serialize(wrongPayload))
            .Create();
        var json = JsonSerializer.Serialize(request);

        // Act - Deserialize with mismatched type (valid JSON but wrong shape)
        var envelope = JsonSerializer.Deserialize<APIGatewayRequestEnvelope<SimplePayload>>(
            json,
            options
        );

        // Assert - Body deserializes with default values since it's valid JSON but wrong shape
        envelope.Should().NotBeNull();
        envelope!.Body.Should().NotBeNull();
        // Properties use default values (null for string, 0 for int)
        envelope.Body.Name.Should().BeNull();
        envelope.Body.Value.Should().Be(0);
    }

    [Fact]
    public void Deserialize_WithInvalidJsonInBody_PropagatesException()
    {
        // Arrange - Body contains malformed JSON
        var options = new JsonSerializerOptions();
        APIGatewayRequestEnvelope<SimplePayload>.RegisterConverter(options.Converters);

        var request = _fixture
            .Build<APIGatewayProxyRequest>()
            .With(r => r.Body, "{invalid json}") // Missing quotes around key
            .Create();
        var json = JsonSerializer.Serialize(request);

        // Act & Assert - Should propagate JsonException
        var act = () =>
            JsonSerializer.Deserialize<APIGatewayRequestEnvelope<SimplePayload>>(json, options);
        act.Should().Throw<JsonException>();
    }

    #endregion

    #region Round-Trip Consistency

    [Fact]
    public void RoundTrip_SimplePayload_DataPreservedCompletelyIntact()
    {
        // Arrange - Start with a complete envelope
        var options = new JsonSerializerOptions();
        APIGatewayRequestEnvelope<SimplePayload>.RegisterConverter(options.Converters);

        var payload = _fixture.Create<SimplePayload>();
        var originalEnvelope = _fixture
            .Build<APIGatewayRequestEnvelope<SimplePayload>>()
            .With(e => e.Body, payload)
            .Create();

        // Act - Serialize then deserialize
        var json = JsonSerializer.Serialize(originalEnvelope, options);
        var roundTripEnvelope = JsonSerializer.Deserialize<
            APIGatewayRequestEnvelope<SimplePayload>
        >(json, options);

        // Assert - Everything should match
        roundTripEnvelope.Should().NotBeNull();
        roundTripEnvelope!.Body.Should().BeEquivalentTo(originalEnvelope.Body);
        roundTripEnvelope.Path.Should().Be(originalEnvelope.Path);
        roundTripEnvelope.HttpMethod.Should().Be(originalEnvelope.HttpMethod);
    }

    [Fact]
    public void RoundTrip_WithCamelCasePolicy_NamedPayloadPreservedWithPolicy()
    {
        // Arrange - Payload with properties that would be affected by CamelCase policy
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        APIGatewayRequestEnvelope<CamelCasePayload>.RegisterConverter(options.Converters);

        var payload = _fixture.Create<CamelCasePayload>();
        var originalEnvelope = _fixture
            .Build<APIGatewayRequestEnvelope<CamelCasePayload>>()
            .With(e => e.Body, payload)
            .Create();

        // Act - Round-trip with camelCase policy
        var json = JsonSerializer.Serialize(originalEnvelope, options);
        var roundTripEnvelope = JsonSerializer.Deserialize<
            APIGatewayRequestEnvelope<CamelCasePayload>
        >(json, options);

        // Assert - Envelope structure preserved (PascalCase), body with camelCase policy applied
        roundTripEnvelope.Should().NotBeNull();
        roundTripEnvelope!.Path.Should().Be(originalEnvelope.Path);
        roundTripEnvelope.HttpMethod.Should().Be(originalEnvelope.HttpMethod);
        roundTripEnvelope.Body.Should().BeEquivalentTo(originalEnvelope.Body);
        roundTripEnvelope.Body.FirstName.Should().Be(payload.FirstName);
    }

    #endregion

    #region Test Helper Classes

    /// <summary>
    ///     Simple payload without any JSON property name attributes. Used to test basic type
    ///     conversion and as baseline for policy tests.
    /// </summary>
    private class SimplePayload
    {
        public string? Name { get; set; }
        public int Value { get; set; }
    }

    /// <summary>
    ///     Payload with PascalCase property names that would be converted to camelCase by a CamelCase
    ///     naming policy. Used to verify that the user's naming policy is applied to the body correctly.
    /// </summary>
    private class CamelCasePayload
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

    #endregion
}

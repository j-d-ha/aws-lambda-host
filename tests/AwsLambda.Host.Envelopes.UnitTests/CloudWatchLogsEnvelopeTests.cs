using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.CloudWatchLogsEvents;
using AutoFixture;
using AwesomeAssertions;
using AwsLambda.Host.Envelopes.CloudWatchLogs;
using AwsLambda.Host.Options;
using JetBrains.Annotations;
using Xunit;

namespace AwsLambda.Host.Envelopes.UnitTests;

[TestSubject(typeof(CloudWatchLogsEnvelope<>))]
public class CloudWatchLogsEnvelopeTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void ExtractPayload_WithSingleRecord_DeserializesDataContent()
    {
        // Arrange
        var payload = _fixture.Create<TestPayload>();
        var envelope = CreateEnvelope(payload);
        var options = new EnvelopeOptions();

        // Act
        envelope.ExtractPayload(options);

        // Assert
        envelope.Awslogs.DataContent.Should().NotBeNull();
        envelope.Awslogs.DataContent!.Content.Should().Be(payload.Content);
        envelope.Awslogs.DataContent.Priority.Should().Be(payload.Priority);
    }

    [Fact]
    public void ExtractPayload_WithCamelCaseNamingPolicy_DeserializesWithCamelCaseProperties()
    {
        // Arrange
        var payload = _fixture.Create<TestPayload>();
        var envelope = CreateEnvelope(
            payload,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );
        var options = new EnvelopeOptions
        {
            JsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            },
        };

        // Act
        envelope.ExtractPayload(options);

        // Assert
        envelope.Awslogs.DataContent.Should().NotBeNull();
        envelope.Awslogs.DataContent!.Content.Should().Be(payload.Content);
        envelope.Awslogs.DataContent.Priority.Should().Be(payload.Priority);
    }

    [Fact]
    public void ExtractPayload_WithNullData_ThrowsArgumentNullException()
    {
        // Arrange
        var envelope = new CloudWatchLogsEnvelope<TestPayload>
        {
            Awslogs = new CloudWatchLogsEnvelopeBase<TestPayload>.LogEnvelope
            {
                EncodedData = null,
            },
        };
        var options = new EnvelopeOptions();

        // Act & Assert
        var act = () => envelope.ExtractPayload(options);
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void ExtractPayload_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange
        var invalidJson = _fixture.Create<string>();
        var envelope = CreateEnvelopeWithRawData(invalidJson);
        var options = new EnvelopeOptions();

        // Act & Assert
        var act = () => envelope.ExtractPayload(options);
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void ExtractPayload_WithValidNullValue_SetsDataContentToNull()
    {
        // Arrange
        var envelope = CreateEnvelopeWithRawData("null");
        var options = new EnvelopeOptions();

        // Act
        envelope.ExtractPayload(options);

        // Assert
        envelope.Awslogs.DataContent.Should().BeNull();
    }

    [Fact]
    public void LogEnvelope_DataContent_HasJsonIgnoreAttribute()
    {
        // Arrange
        var property = typeof(CloudWatchLogsEnvelopeBase<TestPayload>.LogEnvelope).GetProperty(
            nameof(CloudWatchLogsEnvelopeBase<TestPayload>.LogEnvelope.DataContent)
        );

        // Act
        var hasJsonIgnoreAttribute =
            property?.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).Length > 0;

        // Assert
        hasJsonIgnoreAttribute.Should().BeTrue();
    }

    [Fact]
    public void CloudWatchLogsEnvelope_InheritsFromCloudWatchLogsEvent()
    {
        // Arrange & Act
        var envelope = new CloudWatchLogsEnvelope<TestPayload>
        {
            Awslogs = new CloudWatchLogsEnvelopeBase<TestPayload>.LogEnvelope(),
        };

        // Assert
        envelope.Should().BeAssignableTo<CloudWatchLogsEvent>();
    }

    private CloudWatchLogsEnvelope<TestPayload> CreateEnvelope(
        TestPayload payload,
        JsonSerializerOptions? serializerOptions = null
    )
    {
        var json = JsonSerializer.Serialize(payload, serializerOptions);
        return CreateEnvelopeWithRawData(json);
    }

    private CloudWatchLogsEnvelope<TestPayload> CreateEnvelopeWithRawData(string data)
    {
        // CloudWatch Logs data is base64-encoded and gzip-compressed
        var jsonBytes = Encoding.UTF8.GetBytes(data);

        using var outputStream = new MemoryStream();
        using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
            gzipStream.Write(jsonBytes, 0, jsonBytes.Length);

        var compressedData = outputStream.ToArray();
        var base64String = Convert.ToBase64String(compressedData);

        return new CloudWatchLogsEnvelope<TestPayload>
        {
            Awslogs = new CloudWatchLogsEnvelopeBase<TestPayload>.LogEnvelope
            {
                EncodedData = base64String,
            },
        };
    }

    private record TestPayload(string Content, int Priority);
}

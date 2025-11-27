using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.SNSEvents;
using AutoFixture;
using AwesomeAssertions;
using AwsLambda.Host.Envelopes.Sns;
using AwsLambda.Host.Options;
using JetBrains.Annotations;
using Xunit;
using SNSRecord = Amazon.Lambda.SNSEvents.SNSEvent.SNSRecord;

namespace AwsLambda.Host.Envelopes.UnitTests;

[TestSubject(typeof(SnsEnvelope<>))]
public class SnsEnvelopeTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void ExtractPayload_WithSingleRecord_DeserializesBodyContent()
    {
        // Arrange
        var payload = _fixture.Create<MessagePayload>();
        var json = JsonSerializer.Serialize(payload);
        var snsMessage = new SNSEvent.SNS { Message = json };
        var record = new SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope { Sns = snsMessage };
        var envelope = new SnsEnvelope<MessagePayload> { Records = [record] };
        var options = new EnvelopeOptions();

        // Act
        envelope.ExtractPayload(options);

        // Assert
        record.BodyContent.Should().NotBeNull();
        record.BodyContent!.Content.Should().Be(payload.Content);
        record.BodyContent.Priority.Should().Be(payload.Priority);
    }

    [Fact]
    public void ExtractPayload_WithMultipleRecords_DeserializesAllMessages()
    {
        // Arrange
        var payload1 = _fixture.Create<MessagePayload>();
        var payload2 = _fixture.Create<MessagePayload>();
        var payload3 = _fixture.Create<MessagePayload>();
        var json1 = JsonSerializer.Serialize(payload1);
        var json2 = JsonSerializer.Serialize(payload2);
        var json3 = JsonSerializer.Serialize(payload3);

        var record1 = new SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope
        {
            Sns = new SNSEvent.SNS { Message = json1 },
        };
        var record2 = new SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope
        {
            Sns = new SNSEvent.SNS { Message = json2 },
        };
        var record3 = new SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope
        {
            Sns = new SNSEvent.SNS { Message = json3 },
        };

        var envelope = new SnsEnvelope<MessagePayload> { Records = [record1, record2, record3] };
        var options = new EnvelopeOptions();

        // Act
        envelope.ExtractPayload(options);

        // Assert
        record1.BodyContent.Should().NotBeNull();
        record1.BodyContent!.Content.Should().Be(payload1.Content);
        record2.BodyContent.Should().NotBeNull();
        record2.BodyContent!.Content.Should().Be(payload2.Content);
        record3.BodyContent.Should().NotBeNull();
        record3.BodyContent!.Content.Should().Be(payload3.Content);
    }

    [Fact]
    public void ExtractPayload_WithEmptyRecordsList_CompletesWithoutError()
    {
        // Arrange
        var envelope = new SnsEnvelope<MessagePayload> { Records = [] };
        var options = new EnvelopeOptions();

        // Act
        var act = () => envelope.ExtractPayload(options);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ExtractPayload_WithCamelCaseNamingPolicy_DeserializesWithCamelCaseProperties()
    {
        // Arrange
        var payload = _fixture.Create<MessagePayload>();
        var json = JsonSerializer.Serialize(
            payload,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );
        var snsMessage = new SNSEvent.SNS { Message = json };
        var record = new SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope { Sns = snsMessage };
        var envelope = new SnsEnvelope<MessagePayload> { Records = [record] };
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
        record.BodyContent.Should().NotBeNull();
        record.BodyContent!.Content.Should().Be(payload.Content);
        record.BodyContent.Priority.Should().Be(payload.Priority);
    }

    [Fact]
    public void ExtractPayload_WithNullMessage_ThrowsArgumentNullException()
    {
        // Arrange
        var record = new SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope { Sns = new SNSEvent.SNS { Message = null } };
        var envelope = new SnsEnvelope<MessagePayload> { Records = [record] };
        var options = new EnvelopeOptions();

        // Act & Assert
        var act = () => envelope.ExtractPayload(options);
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void ExtractPayload_WithEmptyMessage_ThrowsJsonException()
    {
        // Arrange
        var record = new SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope
        {
            Sns = new SNSEvent.SNS { Message = string.Empty },
        };
        var envelope = new SnsEnvelope<MessagePayload> { Records = [record] };
        var options = new EnvelopeOptions();

        // Act & Assert
        var act = () => envelope.ExtractPayload(options);
        act.Should().ThrowExactly<JsonException>();
    }

    [Fact]
    public void ExtractPayload_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange
        var invalidJson = _fixture.Create<string>();
        var record = new SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope
        {
            Sns = new SNSEvent.SNS { Message = invalidJson },
        };
        var envelope = new SnsEnvelope<MessagePayload> { Records = [record] };
        var options = new EnvelopeOptions();

        // Act & Assert
        var act = () => envelope.ExtractPayload(options);
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void SnsMessageEnvelope_BodyContent_HasJsonIgnoreAttribute()
    {
        // Arrange
        var property = typeof(SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope).GetProperty(
            nameof(SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope.BodyContent)
        );

        // Act
        var hasJsonIgnoreAttribute =
            property?.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).Length > 0;

        // Assert
        hasJsonIgnoreAttribute.Should().BeTrue();
    }

    [Fact]
    public void SnsEnvelope_InheritsFromSnsEvent()
    {
        // Arrange & Act
        var envelope = new SnsEnvelope<MessagePayload> { Records = [] };

        // Assert
        envelope.Should().BeAssignableTo<SNSEvent>();
    }

    [Fact]
    public void SnsMessageEnvelope_InheritsFromSnsRecord()
    {
        // Arrange & Act
        var messageEnvelope = new SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope();

        // Assert
        messageEnvelope.Should().BeAssignableTo<SNSRecord>();
    }

    [Fact]
    public void ExtractPayload_WithMixedValidAndInvalidRecords_StopsAtFirstError()
    {
        // Arrange
        var validPayload = _fixture.Create<MessagePayload>();
        var validJson = JsonSerializer.Serialize(validPayload);
        var validRecord = new SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope
        {
            Sns = new SNSEvent.SNS { Message = validJson },
        };
        var invalidRecord = new SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope
        {
            Sns = new SNSEvent.SNS { Message = "invalid" },
        };

        var envelope = new SnsEnvelope<MessagePayload> { Records = [validRecord, invalidRecord] };
        var options = new EnvelopeOptions();

        // Act & Assert
        var act = () => envelope.ExtractPayload(options);
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void ExtractPayload_WithLargeNumberOfRecords_DeserializesAllSuccessfully()
    {
        // Arrange
        const int recordCount = 100;
        var records = new List<SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope>();

        for (var i = 0; i < recordCount; i++)
        {
            var payload = new MessagePayload($"Message{i}", i);
            var json = JsonSerializer.Serialize(payload);
            records.Add(new SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope
            {
                Sns = new SNSEvent.SNS { Message = json },
            });
        }

        var envelope = new SnsEnvelope<MessagePayload> { Records = records };
        var options = new EnvelopeOptions();

        // Act
        envelope.ExtractPayload(options);

        // Assert
        envelope.Records.Should().HaveCount(recordCount);
        for (var i = 0; i < recordCount; i++)
        {
            var record = envelope.Records[i];
            record.BodyContent.Should().NotBeNull();
            record.BodyContent!.Content.Should().Be($"Message{i}");
            record.BodyContent.Priority.Should().Be(i);
        }
    }

    [Fact]
    public void ExtractPayload_WithValidNullValue_SetsBodyContentToNull()
    {
        // Arrange
        var record = new SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope
        {
            Sns = new SNSEvent.SNS { Message = "null" },
        };
        var envelope = new SnsEnvelope<MessagePayload> { Records = [record] };
        var options = new EnvelopeOptions();

        // Act
        envelope.ExtractPayload(options);

        // Assert
        record.BodyContent.Should().BeNull();
    }

    private record MessagePayload(string Content, int Priority);
}

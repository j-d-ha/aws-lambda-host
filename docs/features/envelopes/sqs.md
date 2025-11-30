# SQS Envelope

Strongly-typed SQS event handling with automatic JSON deserialization of message bodies.

---

## Introduction

The `AwsLambda.Host.Envelopes.Sqs` package provides type-safe wrappers around AWS Lambda's `SQSEvent` class. Instead of manually parsing JSON from `record.Body` strings, you get strongly-typed access to deserialized message payloads via `record.BodyContent<T>`.

**When to use**:

- ✅ Processing SQS messages with structured JSON payloads
- ✅ You want compile-time type safety for message handling
- ✅ You need IDE IntelliSense support for message properties
- ✅ Implementing SNS-to-SQS subscription patterns
- ✅ You want to avoid manual JSON parsing boilerplate

---

## Installation

```bash
dotnet add package AwsLambda.Host.Envelopes.Sqs
```

Requires `AwsLambda.Host` to be installed.

---

## Classes Provided

| Class | Base Class | Use Case |
|-------|-----------|----------|
| **`SqsEnvelope<T>`** | `SQSEvent` | SQS messages with deserialized JSON message bodies |
| **`SqsSnsEnvelope<T>`** | `SqsEnvelopeBase<T>` | SQS messages containing SNS notifications (SNS-to-SQS pattern) |

Both classes extend `SqsEnvelopeBase<T>`, which provides the core envelope functionality.

---

## Quick Start

Define your message type and handler:

```csharp title="Program.cs" linenums="1"
using Amazon.Lambda.SQSEvents;
using AwsLambda.Host.Builder;
using AwsLambda.Host.Envelopes.Sqs;
using Microsoft.Extensions.Logging;

var builder = LambdaApplication.CreateBuilder();
var lambda = builder.Build();

// SqsEnvelope<Message> provides access to SQS event and deserialized Message payloads
lambda.MapHandler(
    ([Event] SqsEnvelope<Message> envelope, ILogger<Program> logger) =>
    {
        // Return SQSBatchResponse to handle partial failures
        var batchResponse = new SQSBatchResponse();

        foreach (var record in envelope.Records)
        {
            // Add failure if message body couldn't be deserialized
            if (record.BodyContent is null)
            {
                batchResponse.BatchItemFailures.Add(
                    new SQSBatchResponse.BatchItemFailure
                    {
                        ItemIdentifier = record.MessageId
                    }
                );
                continue;
            }

            logger.LogInformation(
                "Processing message: {Name}",
                record.BodyContent.Name
            );
        }

        return batchResponse;
    }
);

await lambda.RunAsync();

// Your message payload - deserialized from SQS message body
internal record Message(string Name);
```

---

## Request Envelope (`SqsEnvelope<T>`)

### Basic Usage

The `SqsEnvelope<T>` class extends `SQSEvent` and provides type-safe access to deserialized message payloads:

```csharp title="Program.cs" linenums="1"
lambda.MapHandler(
    ([Event] SqsEnvelope<OrderMessage> envelope, IOrderProcessor processor) =>
    {
        foreach (var record in envelope.Records)
        {
            if (record.BodyContent is null)
            {
                Console.WriteLine($"Failed to deserialize message {record.MessageId}");
                continue;
            }

            // Access strongly-typed properties
            processor.ProcessOrder(
                record.BodyContent.OrderId,
                record.BodyContent.CustomerId,
                record.BodyContent.TotalAmount
            );

            // Access original SQS message properties
            Console.WriteLine($"Message ID: {record.MessageId}");
            Console.WriteLine($"Receipt Handle: {record.ReceiptHandle}");
            Console.WriteLine($"Sent Timestamp: {record.Attributes["SentTimestamp"]}");
        }

        return new SQSBatchResponse();
    }
);

internal record OrderMessage(
    string OrderId,
    string CustomerId,
    decimal TotalAmount,
    DateTime OrderDate
);
```

### Accessing Messages

Each message in `envelope.Records` has:

- **`BodyContent`** (type `T?`) - Deserialized message payload (null if deserialization failed)
- **All `SQSMessage` properties** - `MessageId`, `ReceiptHandle`, `Body`, `Attributes`, `MessageAttributes`, etc.

```csharp title="Accessing message properties"
foreach (var record in envelope.Records)
{
    // Deserialized payload
    var payload = record.BodyContent; // Type: OrderMessage?

    // Original SQS message properties
    var messageId = record.MessageId;
    var receiptHandle = record.ReceiptHandle;
    var rawBody = record.Body; // Original JSON string
    var attributes = record.Attributes;
    var messageAttributes = record.MessageAttributes;
}
```

### Batch Response Handling

SQS supports partial batch failures. Return `SQSBatchResponse` with failed message IDs:

```csharp title="Partial batch failure handling" linenums="1"
lambda.MapHandler(
    ([Event] SqsEnvelope<Message> envelope) =>
    {
        var batchResponse = new SQSBatchResponse();

        foreach (var record in envelope.Records)
        {
            try
            {
                if (record.BodyContent is null)
                    throw new InvalidOperationException("Deserialization failed");

                // Process message
                ProcessMessage(record.BodyContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing {record.MessageId}: {ex.Message}");

                // Mark message as failed - will be retried
                batchResponse.BatchItemFailures.Add(
                    new SQSBatchResponse.BatchItemFailure
                    {
                        ItemIdentifier = record.MessageId
                    }
                );
            }
        }

        return batchResponse;
    }
);
```

!!! tip "Partial Batch Failure Best Practices"
    - Always return `SQSBatchResponse` even if all messages succeed (empty failures list)
    - Add failed message IDs to `BatchItemFailures` - SQS will retry only those messages
    - Failed messages are made visible again and will be redelivered
    - Successful messages are deleted from the queue

---

## SNS-to-SQS Pattern (`SqsSnsEnvelope<T>`)

### When to Use

Use `SqsSnsEnvelope<T>` when:

- SNS topic delivers messages to an SQS queue (SNS-to-SQS subscription)
- Your Lambda function processes messages from that SQS queue
- You need to access both SNS message metadata AND the typed payload

### Two-Stage Deserialization

`SqsSnsEnvelope<T>` performs two-stage deserialization:

1. **Stage 1**: SQS message body → SNS message envelope
2. **Stage 2**: SNS message content → Your payload type `T`

```csharp title="SNS-to-SQS pattern" linenums="1"
using AwsLambda.Host.Envelopes.Sns;

lambda.MapHandler(
    ([Event] SqsSnsEnvelope<Notification> envelope, ILogger<Program> logger) =>
    {
        var batchResponse = new SQSBatchResponse();

        foreach (var record in envelope.Records)
        {
            if (record.BodyContent is null)
            {
                batchResponse.BatchItemFailures.Add(
                    new SQSBatchResponse.BatchItemFailure
                    {
                        ItemIdentifier = record.MessageId
                    }
                );
                continue;
            }

            // Access SNS message envelope
            var snsMessage = record.BodyContent;

            // Access deserialized SNS message content
            var notification = snsMessage.MessageContent;

            logger.LogInformation(
                "SNS Topic: {Topic}, Subject: {Subject}, Message: {Message}",
                snsMessage.TopicArn,
                snsMessage.Subject,
                notification?.Message
            );
        }

        return batchResponse;
    }
);

internal record Notification(string Message, DateTime Timestamp);
```

### SNS Message Properties

The `BodyContent` provides access to SNS message envelope:

```csharp
foreach (var record in envelope.Records)
{
    var snsEnvelope = record.BodyContent; // Type: SnsMessageEnvelope?

    // SNS message metadata
    var topicArn = snsEnvelope.TopicArn;
    var subject = snsEnvelope.Subject;
    var messageId = snsEnvelope.MessageId;
    var timestamp = snsEnvelope.Timestamp;
    var messageAttributes = snsEnvelope.MessageAttributes;

    // Deserialized payload from SNS message
    var payload = snsEnvelope.MessageContent; // Type: Notification?
}
```

---

## Configuration

### Envelope Options

Configure JSON serialization options for envelope payloads:

```csharp title="Program.cs" linenums="1"
using System.Text.Json;

var builder = LambdaApplication.CreateBuilder();

builder.Services.ConfigureEnvelopeOptions(options =>
{
    // Use snake_case for JSON property names
    options.JsonOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;

    // Case-insensitive property matching
    options.JsonOptions.PropertyNameCaseInsensitive = true;

    // Allow trailing commas in JSON
    options.JsonOptions.AllowTrailingCommas = true;
});

var lambda = builder.Build();
```

### Common Configuration Patterns

```csharp title="Configuration examples" linenums="1"
builder.Services.ConfigureEnvelopeOptions(options =>
{
    // Camel case properties
    options.JsonOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

    // Write indented JSON for debugging
    options.JsonOptions.WriteIndented = true;

    // Handle missing properties gracefully
    options.JsonOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

    // Custom number handling
    options.JsonOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
});
```

---

## Native AOT Support

### JsonSerializerContext Registration

For Native AOT compilation, register envelope and payload types in your `JsonSerializerContext`:

```csharp title="Program.cs" linenums="1"
using System.Text.Json.Serialization;

[JsonSerializable(typeof(SqsEnvelope<Message>))]
[JsonSerializable(typeof(Message))]
internal partial class SerializerContext : JsonSerializerContext;
```

### Dual Context Registration

**Critical**: Register the context in **both** the Lambda serializer AND envelope options:

```csharp title="Program.cs" linenums="1"
var builder = LambdaApplication.CreateBuilder();

// 1. Register for Lambda event deserialization
builder.Services.AddLambdaSerializerWithContext<SerializerContext>();

// 2. Register for envelope payload deserialization
builder.Services.ConfigureEnvelopeOptions(options =>
{
    options.JsonOptions.TypeInfoResolver = SerializerContext.Default;
});

var lambda = builder.Build();
```

!!! warning "Why Two Registrations?"
    The context must be registered twice because deserialization happens in two stages:

    1. **Lambda serializer** deserializes the raw `SQSEvent` from Lambda runtime
    2. **Envelope options** deserialize the message body content into your payload type `T`

    Both stages need access to the `JsonSerializerContext` for AOT compilation.

### SNS-to-SQS AOT Configuration

For `SqsSnsEnvelope<T>`, you must register additional types with unique `TypeInfoPropertyName`:

```csharp title="SNS-to-SQS AOT context" linenums="1"
using Amazon.Lambda.SNSEvents;
using Amazon.Lambda.SQSEvents;

[JsonSerializable(typeof(SqsSnsEnvelope<Notification>))]
[JsonSerializable(typeof(SnsEnvelope<Notification>.SnsMessageEnvelope))]
[JsonSerializable(typeof(SNSEvent.MessageAttribute), TypeInfoPropertyName = "SnsMessageAttribute")]
[JsonSerializable(typeof(SQSEvent.MessageAttribute), TypeInfoPropertyName = "SqsMessageAttribute")]
[JsonSerializable(typeof(Notification))]
internal partial class SerializerContext : JsonSerializerContext;
```

!!! danger "MessageAttribute Naming Collision"
    Both `SNSEvent.MessageAttribute` and `SQSEvent.MessageAttribute` have the same type name. Without unique `TypeInfoPropertyName` values, AOT compilation will fail with a naming conflict. Always specify unique names for these types.

---

## Custom Envelopes

### XML Serialization Example

Extend `SqsEnvelopeBase<T>` to support custom serialization formats:

```csharp title="SqsXmlEnvelope.cs" linenums="1"
using System.Xml;
using System.Xml.Serialization;
using AwsLambda.Host.Abstractions.Options;
using AwsLambda.Host.Envelopes.Sqs;

public sealed class SqsXmlEnvelope<T> : SqsEnvelopeBase<T>
{
    private static readonly XmlSerializer Serializer = new(typeof(T));

    public override void ExtractPayload(EnvelopeOptions options)
    {
        foreach (var record in Records)
        {
            using var stringReader = new StringReader(record.Body);
            using var xmlReader = XmlReader.Create(stringReader, options.XmlReaderSettings);
            record.BodyContent = (T?)Serializer.Deserialize(xmlReader);
        }
    }
}
```

**Usage**:

```csharp title="Using XML envelope"
lambda.MapHandler(
    ([Event] SqsXmlEnvelope<XmlMessage> envelope) =>
    {
        foreach (var record in envelope.Records)
        {
            Console.WriteLine($"Deserialized XML: {record.BodyContent?.Data}");
        }

        return new SQSBatchResponse();
    }
);

public class XmlMessage
{
    public string? Data { get; set; }
}
```

### Custom Serialization Formats

You can implement any serialization format by overriding `ExtractPayload`:

```csharp title="Protocol Buffers example (conceptual)"
public sealed class SqsProtobufEnvelope<T> : SqsEnvelopeBase<T> where T : IMessage<T>, new()
{
    public override void ExtractPayload(EnvelopeOptions options)
    {
        var parser = new MessageParser<T>(() => new T());

        foreach (var record in Records)
        {
            var bytes = Convert.FromBase64String(record.Body);
            record.BodyContent = parser.ParseFrom(bytes);
        }
    }
}
```

---

## Common Patterns

### Validation

Validate deserialized payloads before processing:

```csharp title="Payload validation" linenums="1"
lambda.MapHandler(
    ([Event] SqsEnvelope<OrderMessage> envelope, IValidator<OrderMessage> validator) =>
    {
        var batchResponse = new SQSBatchResponse();

        foreach (var record in envelope.Records)
        {
            if (record.BodyContent is null)
            {
                Console.WriteLine($"Deserialization failed for {record.MessageId}");
                batchResponse.BatchItemFailures.Add(
                    new SQSBatchResponse.BatchItemFailure
                    {
                        ItemIdentifier = record.MessageId
                    }
                );
                continue;
            }

            var validationResult = validator.Validate(record.BodyContent);
            if (!validationResult.IsValid)
            {
                Console.WriteLine($"Validation failed: {string.Join(", ", validationResult.Errors)}");
                batchResponse.BatchItemFailures.Add(
                    new SQSBatchResponse.BatchItemFailure
                    {
                        ItemIdentifier = record.MessageId
                    }
                );
                continue;
            }

            // Process valid message
            ProcessOrder(record.BodyContent);
        }

        return batchResponse;
    }
);
```

### Error Handling with Logging

```csharp title="Error handling pattern" linenums="1"
lambda.MapHandler(
    ([Event] SqsEnvelope<Message> envelope, ILogger<Program> logger) =>
    {
        var batchResponse = new SQSBatchResponse();

        foreach (var record in envelope.Records)
        {
            try
            {
                if (record.BodyContent is null)
                {
                    logger.LogError(
                        "Failed to deserialize message {MessageId}. Body: {Body}",
                        record.MessageId,
                        record.Body
                    );
                    throw new InvalidOperationException("Deserialization failed");
                }

                // Process message
                ProcessMessage(record.BodyContent);

                logger.LogInformation(
                    "Successfully processed message {MessageId}",
                    record.MessageId
                );
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error processing message {MessageId}",
                    record.MessageId
                );

                batchResponse.BatchItemFailures.Add(
                    new SQSBatchResponse.BatchItemFailure
                    {
                        ItemIdentifier = record.MessageId
                    }
                );
            }
        }

        return batchResponse;
    }
);
```

### Accessing SQS Message Attributes

```csharp title="Reading message attributes" linenums="1"
foreach (var record in envelope.Records)
{
    // Standard SQS attributes
    var sentTimestamp = record.Attributes.GetValueOrDefault("SentTimestamp");
    var approximateReceiveCount = record.Attributes.GetValueOrDefault("ApproximateReceiveCount");

    // Custom message attributes
    if (record.MessageAttributes.TryGetValue("Priority", out var priorityAttr))
    {
        var priority = priorityAttr.StringValue;
        Console.WriteLine($"Message priority: {priority}");
    }

    // Process with context
    if (int.TryParse(approximateReceiveCount, out var receiveCount))
    {
        if (receiveCount > 3)
        {
            Console.WriteLine($"Message {record.MessageId} has been retried {receiveCount} times");
            // Consider moving to DLQ
        }
    }
}
```

---

## Best Practices

### ✅ Do

- **Always return `SQSBatchResponse`** even if all messages succeed
- **Check for null `BodyContent`** before processing (deserialization can fail)
- **Use partial batch failures** to retry only failed messages
- **Register types for AOT** when using Native AOT compilation
- **Register context in both places** for AOT (Lambda serializer + envelope options)
- **Log deserialization failures** with message ID and body for debugging
- **Use message attributes** for filtering and routing
- **Implement idempotency** - SQS may deliver messages more than once

### ❌ Don't

- **Don't throw unhandled exceptions** - use `SQSBatchResponse.BatchItemFailures` instead
- **Don't process without null checks** - `BodyContent` can be null if deserialization fails
- **Don't retry all messages** when only some fail - use partial batch failures
- **Don't forget dual context registration** for AOT (will cause runtime errors)
- **Don't ignore `ApproximateReceiveCount`** - monitor for poison pill messages
- **Don't skip DLQ configuration** - messages failing repeatedly should go to DLQ

---

## Troubleshooting

### "`BodyContent` is always null"

**Cause**: JSON deserialization is failing silently.

**Solutions**:

1. Check your message payload structure matches the type definition:
   ```csharp
   // Ensure property names match (or use [JsonPropertyName])
   internal record Message(
       [property: JsonPropertyName("message_name")] string MessageName
   );
   ```

2. Enable case-insensitive matching:
   ```csharp
   builder.Services.ConfigureEnvelopeOptions(options =>
   {
       options.JsonOptions.PropertyNameCaseInsensitive = true;
   });
   ```

3. Log raw message body to inspect structure:
   ```csharp
   if (record.BodyContent is null)
   {
       Console.WriteLine($"Failed to deserialize: {record.Body}");
   }
   ```

### "AOT compilation fails with naming conflict"

**Error**: `error CS0102: The type 'JsonContext' already contains a definition for 'MessageAttribute'`

**Cause**: Both `SNSEvent.MessageAttribute` and `SQSEvent.MessageAttribute` registered without unique names.

**Solution**: Use `TypeInfoPropertyName` for `SqsSnsEnvelope`:
```csharp
[JsonSerializable(typeof(SNSEvent.MessageAttribute), TypeInfoPropertyName = "SnsMessageAttribute")]
[JsonSerializable(typeof(SQSEvent.MessageAttribute), TypeInfoPropertyName = "SqsMessageAttribute")]
```

### "Messages retrying indefinitely"

**Cause**: Not using `SQSBatchResponse` to mark failures, or throwing exceptions instead.

**Solution**: Always return `SQSBatchResponse` with failed message IDs:
```csharp
var batchResponse = new SQSBatchResponse();

try
{
    ProcessMessage(record.BodyContent);
}
catch
{
    batchResponse.BatchItemFailures.Add(
        new SQSBatchResponse.BatchItemFailure { ItemIdentifier = record.MessageId }
    );
}

return batchResponse; // Don't throw!
```

---

## Key Takeaways

1. **Type-safe payloads** - `SqsEnvelope<T>` eliminates manual JSON parsing
2. **Two classes** - `SqsEnvelope<T>` for SQS, `SqsSnsEnvelope<T>` for SNS-to-SQS
3. **Partial batch failures** - Return `SQSBatchResponse` with failed message IDs
4. **Null checks required** - `BodyContent` can be null if deserialization fails
5. **AOT needs two registrations** - Lambda serializer AND envelope options
6. **Custom formats supported** - Extend `SqsEnvelopeBase<T>` for XML, Protobuf, etc.
7. **SNS-to-SQS requires special handling** - Unique `TypeInfoPropertyName` for MessageAttribute types

---

## Next Steps

- **[Envelope Pattern Overview](index.md)** - Learn how all envelopes work
- **[SNS Envelope](sns.md)** - Process SNS notifications with type safety
- **[API Gateway Envelope](api-gateway.md)** - Handle API Gateway requests/responses
- **[Configuration Guide](../../guides/configuration.md)** - Configure envelope serialization options
- **[Error Handling Guide](../../guides/error-handling.md)** - Error handling patterns for Lambda
- **[Deployment Guide](../../guides/deployment.md)** - Deploy SQS-triggered Lambda functions

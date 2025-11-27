# AwsLambda.Host.Envelopes.Kafka

Strongly-typed Kafka event envelope for AWS Lambda, providing type-safe access to message payloads delivered through AWS Managed Streaming for Apache Kafka (MSK) or self-managed Apache Kafka.

## Overview

This package extends the official `Amazon.Lambda.KafkaEvents.KafkaEvent` with a `ValueContent<T>` property on each record, enabling automatic deserialization of base64-encoded Kafka message values into your custom types.

## Quick Start

```csharp
using AwsLambda.Host;
using AwsLambda.Host.Envelopes.Kafka;

var builder = LambdaHost.CreateApplicationBuilder(args);

builder.Services.AddLambdaHandler<KafkaEnvelope<OrderEvent>, ILambdaContext, Task>(
    async (request, context) =>
    {
        foreach (var topic in request.Records)
        {
            context.Logger.LogInformation($"Processing {topic.Value.Count} records from topic: {topic.Key}");

            foreach (var record in topic.Value)
            {
                var order = record.ValueContent; // Strongly-typed OrderEvent
                context.Logger.LogInformation($"Order ID: {order?.OrderId}, Amount: {order?.Amount}");

                // Process your order...
            }
        }

        await Task.CompletedTask;
    }
);

var app = builder.Build();
await app.RunAsync();

public record OrderEvent(string OrderId, decimal Amount, DateTime Timestamp);
```

## Custom Envelopes

Create custom envelope classes for alternative serialization formats (XML, Protobuf, etc.):

```csharp
public sealed class KafkaXmlEnvelope<T> : KafkaEnvelopeBase<T>
{
    public override void ExtractPayload(EnvelopeOptions options)
    {
        var serializer = new XmlSerializer(typeof(T));

        foreach (var topic in Records)
        {
            foreach (var record in topic.Value)
            {
                using var reader = new StreamReader(record.Value, Encoding.UTF8, leaveOpen: true);
                var base64String = reader.ReadToEnd();
                var xmlBytes = Convert.FromBase64String(base64String);

                using var xmlStream = new MemoryStream(xmlBytes);
                using var xmlReader = XmlReader.Create(xmlStream, options.XmlReaderSettings);

                record.ValueContent = (T?)serializer.Deserialize(xmlReader);
            }
        }
    }
}
```

## AOT Support

For Native AOT compilation, register your JSON types with a `JsonSerializerContext`:

```csharp
[JsonSerializable(typeof(OrderEvent))]
[JsonSerializable(typeof(KafkaEnvelope<OrderEvent>))]
public partial class AppJsonContext : JsonSerializerContext { }

// Configure in your Lambda handler
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default);
});
```

## Related Packages

- [AwsLambda.Host.Envelopes.Kinesis](../AwsLambda.Host.Envelopes.Kinesis) - Kinesis stream envelopes
- [AwsLambda.Host.Envelopes.KinesisFirehose](../AwsLambda.Host.Envelopes.KinesisFirehose) - Kinesis Firehose envelopes
- [AwsLambda.Host.Envelopes.Sns](../AwsLambda.Host.Envelopes.Sns) - SNS message envelopes
- [AwsLambda.Host.Envelopes.Sqs](../AwsLambda.Host.Envelopes.Sqs) - SQS message envelopes
- [AwsLambda.Host.Envelopes.ApiGateway](../AwsLambda.Host.Envelopes.ApiGateway) - API Gateway request/response envelopes
- [AwsLambda.Host.Envelopes.Alb](../AwsLambda.Host.Envelopes.Alb) - Application Load Balancer envelopes

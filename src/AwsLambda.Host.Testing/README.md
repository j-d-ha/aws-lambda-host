# AwsLambda.Host.Testing

AWS Lambda Local Invocation - Happy Path
```text
========== HTTP REQUEST ==========
GET http://localhost:5050/2018-06-01/runtime/invocation/next
Version: 1.1

Headers:
  Accept: application/json

========== HTTP RESPONSE ==========
Status: 200 OK
Version: 1.1

Headers:
  Date: Thu, 04 Dec 2025 20:40:53 GMT
  Server: Kestrel
  Transfer-Encoding: chunked
  Lambda-Runtime-Deadline-Ms: 1764881754010
  Lambda-Runtime-Aws-Request-Id: 000000000002
  Lambda-Runtime-Trace-Id: 2a159b6d-ca3c-4991-8533-c2b2a8da0640
  Lambda-Runtime-Invoked-Function-Arn: arn:aws:lambda:us-west-2:123412341234:function:Function

Content Headers:
  Content-Type: application/json

Body:
"James"
===================================

========== HTTP REQUEST ==========
POST http://localhost:5050/2018-06-01/runtime/invocation/000000000002/response
Version: 1.1

Headers:
  Accept: application/json

Content Headers:
  Content-Type: application/json

Body:
"Hello James!"

========== HTTP RESPONSE ==========
Status: 202 Accepted
Version: 1.1

Headers:
  Date: Thu, 04 Dec 2025 20:40:53 GMT
  Server: Kestrel
  Transfer-Encoding: chunked

Content Headers:
  Content-Type: application/json; charset=utf-8

Body:
{"status":"success"}
===================================

========== HTTP REQUEST ==========
GET http://localhost:5050/2018-06-01/runtime/invocation/next
Version: 1.1

Headers:
  Accept: application/json


```

AWS Lambda Local Invocation - Error Path
```text
========== HTTP REQUEST ==========
GET http://localhost:5050/2018-06-01/runtime/invocation/next
Version: 1.1

Headers:
  Accept: application/json

========== HTTP RESPONSE ==========
Status: 200 OK
Version: 1.1

Headers:
  Date: Thu, 04 Dec 2025 20:44:06 GMT
  Server: Kestrel
  Transfer-Encoding: chunked
  Lambda-Runtime-Deadline-Ms: 1764881946613
  Lambda-Runtime-Aws-Request-Id: 000000000004
  Lambda-Runtime-Trace-Id: 849e7f8f-6a67-4132-b371-7740c9ad9084
  Lambda-Runtime-Invoked-Function-Arn: arn:aws:lambda:us-west-2:123412341234:function:Function

Content Headers:
  Content-Type: application/json

Body:
2
===================================

========== HTTP REQUEST ==========
POST http://localhost:5050/2018-06-01/runtime/invocation/000000000004/error
Version: 1.1

Headers:
  Lambda-Runtime-Function-Error-Type: JsonSerializerException
  Lambda-Runtime-Function-XRay-Error-Cause:   {  "working_directory": "/Users/jonasha/Repos/CSharp/dotnet-lambda-host/examples/AwsLambda.Host.Examples.Testing/bin/Debug/net10.0",  "exceptions": [     {    "type": "JsonSerializerException",    "message": "Error converting the Lambda event JSON payload to type System.String: The JSON value could not be converted to System.String. Path: $ | LineNumber: 0 | BytePositionInLine: 1.",    "stack":       [      {      "label": "AbstractLambdaJsonSerializer.Deserialize"      },      {      "path": "/Users/jonasha/Repos/CSharp/dotnet-lambda-host/src/AwsLambda.Host/Core/Features/DefaultEventFeature.cs",      "label": "DefaultEventFeature`1.GetEvent",      "line": 28      },      {      "path": "/Users/jonasha/Repos/CSharp/dotnet-lambda-host/src/AwsLambda.Host/Core/Features/DefaultEventFeature.cs",      "label": "DefaultEventFeature`1.AwsLambda.Host.Core.IEventFeature.GetEvent",      "line": 35      },      {      "path": "/Users/jonasha/Repos/CSharp/dotnet-lambda-host/src/AwsLambda.Host/Builder/Middleware/RequestEnvelopeMiddleware.cs",      "label": "<<UseExtractAndPackEnvelope>b__2>d.MoveNext",      "line": 45      },      {      "label": "ExceptionDispatchInfo.Throw"      },      {      "label": "TaskAwaiter.ThrowForNonSuccess"      },      {      "label": "TaskAwaiter.HandleNonSuccessAndDebuggerNotification"      },      {      "path": "/Users/jonasha/Repos/CSharp/dotnet-lambda-host/src/AwsLambda.Host/Runtime/LambdaHandlerComposer.cs",      "label": "<<CreateHandler>g__CreateRequestHandler|0>d.MoveNext",      "line": 78      },      {      "label": "ExceptionDispatchInfo.Throw"      },      {      "path": "/Users/jonasha/Repos/CSharp/dotnet-lambda-host/src/AwsLambda.Host/Runtime/LambdaHandlerComposer.cs",      "label": "<<CreateHandler>g__CreateRequestHandler|0>d.MoveNext",      "line": 84      },      {      "label": "ExceptionDispatchInfo.Throw"      },      {      "label": "TaskAwaiter.ThrowForNonSuccess"      },      {      "label": "TaskAwaiter.HandleNonSuccessAndDebuggerNotification"      },      {      "label": "TaskAwaiter`1.GetResult"      },      {      "label": "<<GetHandlerWrapper>b__0>d.MoveNext"      },      {      "label": "ExceptionDispatchInfo.Throw"      },      {      "label": "TaskAwaiter.ThrowForNonSuccess"      },      {      "label": "TaskAwaiter.HandleNonSuccessAndDebuggerNotification"      },      {      "label": "TaskAwaiter`1.GetResult"      },      {      "label": "<<InvokeOnceAsync>b__0>d.MoveNext"      }      ]    } ],  "paths":     ["/Users/jonasha/Repos/CSharp/dotnet-lambda-host/src/AwsLambda.Host/Core/Features/DefaultEventFeature.cs","/Users/jonasha/Repos/CSharp/dotnet-lambda-host/src/AwsLambda.Host/Builder/Middleware/RequestEnvelopeMiddleware.cs","/Users/jonasha/Repos/CSharp/dotnet-lambda-host/src/AwsLambda.Host/Runtime/LambdaHandlerComposer.cs"    ]  }
  Accept: application/json

Content Headers:
  Content-Type: application/vnd.aws.lambda.error+json

Body:
{
  "errorType": "JsonSerializerException",
  "errorMessage": "Error converting the Lambda event JSON payload to type System.String: The JSON value could not be converted to System.String. Path: $ | LineNumber: 0 | BytePositionInLine: 1.",
  "stackTrace": [
    "at Amazon.Lambda.Serialization.SystemTextJson.AbstractLambdaJsonSerializer.Deserialize[T](Stream requestStream)",
    "at AwsLambda.Host.Core.DefaultEventFeature`1.GetEvent(ILambdaHostContext context) in /Users/jonasha/Repos/CSharp/dotnet-lambda-host/src/AwsLambda.Host/Core/Features/DefaultEventFeature.cs:line 28",
    "at AwsLambda.Host.Core.DefaultEventFeature`1.AwsLambda.Host.Core.IEventFeature.GetEvent(ILambdaHostContext context) in /Users/jonasha/Repos/CSharp/dotnet-lambda-host/src/AwsLambda.Host/Core/Features/DefaultEventFeature.cs:line 35",
    "at AwsLambda.Host.Builder.RequestEnvelopeMiddleware.<>c__DisplayClass1_1.<<UseExtractAndPackEnvelope>b__2>d.MoveNext() in /Users/jonasha/Repos/CSharp/dotnet-lambda-host/src/AwsLambda.Host/Builder/Middleware/RequestEnvelopeMiddleware.cs:line 45",
    "--- End of stack trace from previous location ---",
    "at AwsLambda.Host.Runtime.LambdaHandlerComposer.<>c__DisplayClass6_0.<<CreateHandler>g__CreateRequestHandler|0>d.MoveNext() in /Users/jonasha/Repos/CSharp/dotnet-lambda-host/src/AwsLambda.Host/Runtime/LambdaHandlerComposer.cs:line 78",
    "--- End of stack trace from previous location ---",
    "at AwsLambda.Host.Runtime.LambdaHandlerComposer.<>c__DisplayClass6_0.<<CreateHandler>g__CreateRequestHandler|0>d.MoveNext() in /Users/jonasha/Repos/CSharp/dotnet-lambda-host/src/AwsLambda.Host/Runtime/LambdaHandlerComposer.cs:line 84",
    "--- End of stack trace from previous location ---",
    "at Amazon.Lambda.RuntimeSupport.HandlerWrapper.<>c__DisplayClass19_0.<<GetHandlerWrapper>b__0>d.MoveNext()",
    "--- End of stack trace from previous location ---",
    "at Amazon.Lambda.RuntimeSupport.LambdaBootstrap.<>c__DisplayClass26_0.<<InvokeOnceAsync>b__0>d.MoveNext()"
  ],
  "cause":   {
    "errorType": "JsonException",
    "errorMessage": "The JSON value could not be converted to System.String. Path: $ | LineNumber: 0 | BytePositionInLine: 1.",
    "stackTrace": [
      "at System.Text.Json.ThrowHelper.ReThrowWithPath(ReadStack& state, Utf8JsonReader& reader, Exception ex)",
      "at System.Text.Json.Serialization.JsonConverter`1.ReadCore(Utf8JsonReader& reader, T& value, JsonSerializerOptions options, ReadStack& state)",
      "at System.Text.Json.Serialization.Metadata.JsonTypeInfo`1.Deserialize(Utf8JsonReader& reader, ReadStack& state)",
      "at System.Text.Json.JsonSerializer.ReadFromSpan[TValue](ReadOnlySpan`1 utf8Json, JsonTypeInfo`1 jsonTypeInfo, Nullable`1 actualByteCount)",
      "at Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer.InternalDeserialize[T](Byte[] utf8Json)",
      "at Amazon.Lambda.Serialization.SystemTextJson.AbstractLambdaJsonSerializer.Deserialize[T](Stream requestStream)"
    ],
    "cause":     {
      "errorType": "InvalidOperationException",
      "errorMessage": "Cannot get the value of a token type 'Number' as a string.",
      "stackTrace": [
        "at System.Text.Json.ThrowHelper.ThrowInvalidOperationException_ExpectedString(JsonTokenType tokenType)",
        "at System.Text.Json.Utf8JsonReader.GetString()",
        "at System.Text.Json.Serialization.JsonConverter`1.TryRead(Utf8JsonReader& reader, Type typeToConvert, JsonSerializerOptions options, ReadStack& state, T& value, Boolean& isPopulatedValue)",
        "at System.Text.Json.Serialization.JsonConverter`1.ReadCore(Utf8JsonReader& reader, T& value, JsonSerializerOptions options, ReadStack& state)"
      ]
    }
  }
}


========== HTTP RESPONSE ==========
Status: 202 Accepted
Version: 1.1

Headers:
  Date: Thu, 04 Dec 2025 20:44:06 GMT
  Server: Kestrel
  Transfer-Encoding: chunked

Content Headers:
  Content-Type: application/json; charset=utf-8

Body:
{"status":"success"}
===================================

========== HTTP REQUEST ==========
GET http://localhost:5050/2018-06-01/runtime/invocation/next
Version: 1.1

Headers:
  Accept: application/json


```
using Microsoft.Extensions.Hosting;
using MinimalLambda.Builder;

var builder = LambdaApplication.CreateBuilder();

await using var lambda = builder.Build();

lambda.MapHandler(() => new Response("Hello World!", DateTime.UtcNow));

await lambda.RunAsync();

public class NoEventLambda;

internal record Response(string Message, DateTime TimestampUtc);

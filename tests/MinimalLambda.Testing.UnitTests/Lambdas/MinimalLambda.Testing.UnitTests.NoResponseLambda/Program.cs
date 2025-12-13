using Microsoft.Extensions.Hosting;
using MinimalLambda.Builder;

var builder = LambdaApplication.CreateBuilder();

await using var lambda = builder.Build();

lambda.MapHandler(([Event] NoResponseLambdaRequest request) => { });

await lambda.RunAsync();

public class NoResponseLambda;

internal record NoResponseLambdaRequest(string Name);

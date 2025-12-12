using Microsoft.Extensions.Hosting;
using MinimalLambda.Builder;

var builder = LambdaApplication.CreateBuilder();

await using var lambda = builder.Build();

lambda.MapHandler(([Event] string name) => $"Hello {name}!");

await lambda.RunAsync();

public class SimpleLambda;

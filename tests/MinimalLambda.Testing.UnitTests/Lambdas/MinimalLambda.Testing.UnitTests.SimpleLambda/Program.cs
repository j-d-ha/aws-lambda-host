using Microsoft.Extensions.Hosting;
using MinimalLambda.Builder;

var builder = LambdaApplication.CreateBuilder();

await using var lambda = builder.Build();

lambda.MapHandler(
    ([Event] string name) =>
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exception("Name is required");

        return $"Hello {name}!";
    }
);

await lambda.RunAsync();

public class SimpleLambda;

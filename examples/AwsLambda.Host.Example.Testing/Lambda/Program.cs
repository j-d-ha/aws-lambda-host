using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MinimalLambda.Builder;

// Create the application builder
var builder = LambdaApplication.CreateBuilder();

builder.Services.ConfigureLambdaHostOptions(options =>
{
    options.ClearLambdaOutputFormatting = true;
});

// Build the Lambda application
var lambda = builder.Build();

// throw new Exception("Init failed");

// lambda.OnInit(() =>
// {
//     // throw new Exception("Init failed");
//     // return false;
// });

// Map your handler - the event is automatically injected
// lambda.MapHandler(
//     async ([Event] string name, ILambdaHostContext context, CancellationToken cancellationToken)
// =>
//     {
//         await Task.Delay(TimeSpan.FromSeconds(60), cancellationToken);
//         if (string.IsNullOrWhiteSpace(name))
//             throw new ArgumentNullException(nameof(name), "Name is required.");
//
//         return $"Hello {name}!";
//     }
// );

// lambda.MapHandler(() => "Hello World!");

lambda.MapHandler(
    ([Event] string name) =>
    {
        if (name != "world")
            throw new Exception("bad");
    }
);

lambda.OnShutdown(() =>
{
    Console.WriteLine("Shutdown");
});

// Run the Lambda
await lambda.RunAsync();

public partial class Program;

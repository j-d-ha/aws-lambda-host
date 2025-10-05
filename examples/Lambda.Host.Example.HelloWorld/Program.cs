using Lambda.Host;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder();

var lambda = builder.Build();

lambda.MapHandler(([Request] string input1, [Request] string input2) => "hello world");

await lambda.RunAsync();

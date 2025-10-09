using Lambda.Host;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder<Host>();

var lambda = builder.Build();

lambda.MapHandler(() => "hello world");

await lambda.RunAsync();

[StartupHost]
public partial class Host : LambdaHost;

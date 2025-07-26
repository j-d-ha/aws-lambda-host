using Lambda.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder();

builder.Services.AddSingleton<IService, Service>();
;

var lambda = builder.Build();

lambda.MapHandler(
    async ([Request] string input, IService service) => (await service.GetMessage()).ToUpper()
);

await lambda.RunAsync();

// await LambdaBootstrapBuilder
//     .Create(() => "hello world", new DefaultLambdaJsonSerializer())
//     .Build()
//     .RunAsync();


public interface IService
{
    Task<string> GetMessage();
}

public class Service : IService
{
    public Task<string> GetMessage() => Task.FromResult("hello world");
}

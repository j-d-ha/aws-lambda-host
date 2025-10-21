using System.Threading;
using System.Threading.Tasks;
using Lambda.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder();

builder.Services.AddSingleton<IService, Service>();

var lambda = builder.Build();

lambda.MapHandler(
    async ([Event] Request request, IService service, CancellationToken cancellationToken) =>
        new Response(await service.SayHello(request.Name, cancellationToken))
);

await lambda.RunAsync();

record Request(string Name);

record Response(string Message);

internal interface IService
{
    Task<string> SayHello(string name, CancellationToken cancellationToken);
}

internal class Service : IService
{
    public Task<string> SayHello(string name, CancellationToken cancellationToken) =>
        Task.FromResult($"hello {name}");
}

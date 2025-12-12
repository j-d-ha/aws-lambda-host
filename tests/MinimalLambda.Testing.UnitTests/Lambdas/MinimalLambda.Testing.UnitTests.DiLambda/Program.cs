using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MinimalLambda.Builder;

var builder = LambdaApplication.CreateBuilder();

builder.Services.AddSingleton<ILifecycleService, LifecycleService>();
builder.Services.AddSingleton<IService, Service>();

await using var lambda = builder.Build();

lambda.OnInit((ILifecycleService service) => service.OnStart());

lambda.MapHandler(
    ([Event] Request request, IService service) =>
        new Response(service.GetMessage(request.Name), DateTime.UtcNow)
);

lambda.OnShutdown((ILifecycleService service) => service.OnStop());

await lambda.RunAsync();

public class DiLambda;

internal record Request(string Name);

internal record Response(string Message, DateTime TimestampUtc);

internal interface IService
{
    string GetMessage(string name);
}

internal class Service : IService
{
    public string GetMessage(string name) => $"Hello {name}!";
}

internal interface ILifecycleService
{
    bool OnStart();
    void OnStop();
}

internal class LifecycleService : ILifecycleService
{
    public bool OnStart() => true;

    public void OnStop() { }
}

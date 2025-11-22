using System;
using AwsLambda.Host.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder();

builder.Services.AddSingleton<IService, Service>();

var lambda = builder.Build();

lambda.UseMiddleware(
    async (context, next) =>
    {
        Console.WriteLine("[Middleware 1] Before");
        await next(context);
        Console.WriteLine("[Middleware 1] After");
    }
);

lambda.MapHandler(
    ([Event] Request request, IService service) => new Response(service.GetMessage(request.Name))
);

await lambda.RunAsync();

internal record Response(string Message);

internal record Request(string Name);

internal interface IService
{
    string GetMessage(string name);
}

internal class Service : IService
{
    public string GetMessage(string name) => $"hello {name}";
}

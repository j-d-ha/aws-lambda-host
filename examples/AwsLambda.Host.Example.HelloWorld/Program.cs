using System.Threading.Tasks;
using AwsLambda.Host;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder();

var lambda = builder.Build();

lambda.MapHandler(
    async ([Event] string input, IService service) => (await service.GetMessage()).ToUpper()
);

await lambda.RunAsync();

public interface IService
{
    Task<string> GetMessage();
}

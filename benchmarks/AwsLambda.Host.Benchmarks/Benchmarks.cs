using AwsLambda.Host.Builder;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Hosting;

namespace AwsLambda.Host.Benchmarks;

public class CreateBuilderBenchmarks
{
    [Benchmark]
    public void CreateBuilder()
    {
        var builder = LambdaApplication.CreateBuilder();
        builder.Build();
    }

    [Benchmark]
    public void CreateEmptyBuilder()
    {
        var builder = LambdaApplication.CreateEmptyBuilder(new HostApplicationBuilderSettings());
        builder.Build();
    }
}

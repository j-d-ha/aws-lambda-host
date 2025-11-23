using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

var config = DefaultConfig.Instance;

// var summary = BenchmarkRunner.Run<Benchmarks>(config, args);

var summaries = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);

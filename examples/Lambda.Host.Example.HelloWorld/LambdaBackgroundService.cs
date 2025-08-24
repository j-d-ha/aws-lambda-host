// using Amazon.Lambda.RuntimeSupport;
// using Amazon.Lambda.Serialization.SystemTextJson;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
//
// namespace Lambda.Host.Example.HelloWorld;
//
// public class LambdaBackgroundService2
// {
//     private readonly DelegateHolder _delegateHolder;
//     private readonly IService _service;
//
//     public LambdaBackgroundService2(DelegateHolder delegateHolder, IService service)
//     {
//         _delegateHolder = delegateHolder;
//         _service = service;
//     }
//
//     public async Task StartAsync(CancellationToken cancellationToken)
//     {
//         if (_delegateHolder.Handler is not Func<string, IService, string> lambdaHandler)
//             throw new InvalidOperationException("Invalid handler type.");
//
//         await LambdaBootstrapBuilder
//             .Create(
//                 (string input) => lambdaHandler(input, _service),
//                 new DefaultLambdaJsonSerializer()
//             )
//             .Build()
//             .RunAsync(cancellationToken);
//     }
//
//     public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
// }
//
// // public static class BackgroundServiceExtensions
// // {
// //     public static IServiceCollection AddLambdaService(this IServiceCollection services)
// //     {
// //         services.AddSingleton<IHostedService, LambdaBackgroundService2>();
// //         return services;
// //     }
// // }

using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using AwsLambda.Host.Core.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AwsLambda.Host;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddLambdaHostCoreServices()
        {
            ArgumentNullException.ThrowIfNull(services);

            // register core factories
            services.AddSingleton<IInvocationBuilderFactory, DefaultInvocationBuilderFactory>();
            services.AddSingleton<IOnInitBuilderFactory, DefaultOnInitBuilderFactory>();
            services.AddSingleton<IOnShutdownBuilderFactory, DefaultOnShutdownBuilderFactory>();
            services.AddSingleton<IFeatureCollectionFactory, DefaultFeatureCollectionFactory>();

            // Register internal Lambda execution components
            services.AddSingleton<ILambdaHandlerFactory, LambdaHandlerComposer>();
            services.AddSingleton<ILambdaBootstrapOrchestrator, LambdaBootstrapAdapter>();
            services.AddSingleton<ILambdaLifecycleOrchestrator, LambdaLifecycleOrchestrator>();

            // Register LambdaHostedService as IHostedService
            services.AddHostedService<LambdaHostedService>();

            return services;
        }

        public IServiceCollection TryAddLambdaHostDefaultServices()
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddSingleton<ILambdaSerializer, DefaultLambdaJsonSerializer>();
            services.TryAddSingleton<
                ILambdaCancellationFactory,
                DefaultLambdaCancellationFactory
            >();

            return services;
        }
    }
}

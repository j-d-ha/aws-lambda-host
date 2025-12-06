using Microsoft.Extensions.DependencyInjection;

namespace AwsLambda.Host.Builder.Extensions;

public static class LambdaHttpClientServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddLambdaBootstrapHttpClient<T>()
            where T : HttpClient
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddKeyedSingleton<HttpClient, T>(typeof(ILambdaBootstrapOrchestrator));

            return services;
        }

        public IServiceCollection AddLambdaBootstrapHttpClient<T>(T client)
            where T : HttpClient
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddKeyedSingleton<HttpClient>(typeof(ILambdaBootstrapOrchestrator), client);

            return services;
        }

        public IServiceCollection AddLambdaBootstrapHttpClient(
            Func<IServiceProvider, object?, HttpClient> factory
        )
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddKeyedSingleton<HttpClient>(typeof(ILambdaBootstrapOrchestrator), factory);

            return services;
        }
    }
}

using AwsLambda.Host.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AwsLambda.Host;

public static class EnvelopeMiddleware
{
    extension(ILambdaApplication application)
    {
        public ILambdaApplication UseExtractAndPackEnvelope()
        {
            ArgumentNullException.ThrowIfNull(application);

            var settings = application
                .Services.GetRequiredService<IOptions<EnvelopeOptions>>()
                .Value;

            application.UseMiddleware(
                async (context, next) =>
                {
                    if (context.Event is IEnvelope eventEnvelope)
                        eventEnvelope.ExtractPayload(settings);

                    await next(context);

                    if (context.Response is IEnvelope responseEnvelope)
                        responseEnvelope.PackPayload(settings);
                }
            );

            return application;
        }
    }
}

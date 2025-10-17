using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;

namespace Lambda.Host;

/// <summary>
///     Options for configuring Lambda hosting behavior.
/// </summary>
public class LambdaHostSettings
{
    /// <summary>
    ///     Gets or sets the buffer duration subtracted from the Lambda function's remaining
    ///     execution time when creating cancellation tokens.
    /// </summary>
    /// <remarks>Default is 3 seconds.</remarks>
    public TimeSpan CancellationBuffer { get; set; } = TimeSpan.FromSeconds(3);

    /// <summary>
    ///     Gets or sets the Lambda serializer. If null, defaults to
    ///     <see cref="Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer" />.
    /// </summary>
    public ILambdaSerializer Serializer { get; set; } = new DefaultLambdaJsonSerializer();
}

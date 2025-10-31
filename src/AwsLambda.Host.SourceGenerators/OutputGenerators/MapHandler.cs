using System.Linq;
using AwsLambda.Host.SourceGenerators.Models;

namespace AwsLambda.Host.SourceGenerators;

internal static class MapHandlerSources
{
    internal static string? Generate(MapHandlerInvocationInfo mapHandlerInvocationInfo)
    {
        var delegateInfo = mapHandlerInvocationInfo.DelegateInfo;

        var delegateArguments = delegateInfo
            .Parameters.Select(p => p.Type)
            .Concat(
                new[] { delegateInfo.ResponseType }.Where(t => t != null && t != TypeConstants.Void)
            )
            .ToList();

        var handlerArgs = delegateInfo
            .Parameters.Select(
                (param, index) =>
                    new
                    {
                        VarName = $"arg{index}",
                        AssignmentStatement = param.Source switch
                        {
                            // Event -> deserialize to type
                            ParameterSource.Event => $"context.GetEventT<{param.Type}>()",

                            // ILambdaContext OR ILambdaHostContext -> use context directly
                            ParameterSource.Context => "context",

                            // CancellationToken -> get from context
                            ParameterSource.ContextCancellation => "context.CancellationToken",

                            // inject keyed service from the DI container
                            ParameterSource.KeyedService =>
                                $"context.ServiceProvider.GetRequiredKeyedService<{param.Type}>(\"{param.KeyedServiceKey}\")",

                            // default: inject service from the DI container
                            _ => $"context.ServiceProvider.GetRequiredService<{param.Type}>()",
                        },
                    }
            )
            .ToArray();

        var shouldAwait = delegateInfo.ResponseType.StartsWith(TypeConstants.Task);

        var inputEvent = delegateInfo.EventParameter is { } p
            ? new { IsStream = p.Type == TypeConstants.Stream, p.Type }
            : null;

        // 1. if Action -> no return
        // 3. if Func + Task return type + async -> no return
        // 2. if Func + Task return type -> return value
        // 4. if Func + non-Task return type -> return value
        var hasReturnValue = delegateInfo switch
        {
            { DelegateType: TypeConstants.Action } => false,
            { DelegateType: TypeConstants.Func, ResponseType: TypeConstants.Task } => false,
            _ => true,
        };

        var outputResponse = hasReturnValue
            ? new
            {
                ResponseIsStream = delegateInfo.ResponseType == TypeConstants.Stream,
                ResponseType = delegateInfo.UnwrappedResponseType,
                ResponseTypeIsNullable = delegateInfo.UnwrappedResponseType.EndsWith("?"),
            }
            : null;

        var model = new
        {
            Location = mapHandlerInvocationInfo.InterceptableLocationInfo,
            delegateInfo.DelegateType,
            DelegateArgs = delegateArguments,
            HandlerArgs = handlerArgs,
            ShouldAwait = shouldAwait,
            InputEvent = inputEvent,
            OutputResponse = outputResponse,
        };

        var template = TemplateHelper.LoadTemplate(
            GeneratorConstants.LambdaHostMapHandlerExtensionsTemplateFile
        );

        return template.Render(model);
    }
}

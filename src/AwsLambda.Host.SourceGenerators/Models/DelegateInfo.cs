using System.Linq;
using AwsLambda.Host.SourceGenerators.Types;

namespace AwsLambda.Host.SourceGenerators.Models;

internal readonly record struct DelegateInfo(
    string ResponseType = TypeConstants.Void,
    EquatableArray<ParameterInfo> Parameters = new()
)
{
    internal readonly string UnwrappedResponseType = GetUnwrappedResponseType(ResponseType);

    internal string DelegateType =>
        ResponseType == TypeConstants.Void ? TypeConstants.Action : TypeConstants.Func;

    internal bool HasResponse => ResponseType is not (TypeConstants.Void or TypeConstants.Task);

    internal ParameterInfo? EventParameter =>
        Parameters
            .Where(p => p.Source == ParameterSource.Event)
            .Select(p => (ParameterInfo?)p)
            .FirstOrDefault();

    internal bool HasEventParameter => EventParameter is not null;

    private static string GetUnwrappedResponseType(string responseType)
    {
        // Unwrap Task<T>
        if (responseType.StartsWith(TypeConstants.Task + "<"))
        {
            var startIndex = responseType.IndexOf('<') + 1;
            var endIndex = responseType.LastIndexOf('>');
            responseType = responseType.Substring(startIndex, endIndex - startIndex);
        }

        return responseType;
    }
}

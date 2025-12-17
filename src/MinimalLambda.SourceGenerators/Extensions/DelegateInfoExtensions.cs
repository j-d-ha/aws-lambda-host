using System.Linq;
using System.Text;
using MinimalLambda.SourceGenerators.Models;

namespace MinimalLambda.SourceGenerators.Extensions;

internal static class DelegateInfoExtensions
{
    extension(DelegateInfo delegateInfo)
    {
        internal string BuildHandlerCastCall()
        {
            var signatureBuilder = new StringBuilder();
            signatureBuilder.Append("Utilities.Cast(handler, ");

            signatureBuilder.Append(delegateInfo.ReturnTypeInfo.FullyQualifiedType);

            signatureBuilder.Append(" (");

            signatureBuilder.Append(
                string.Join(
                    ", ",
                    delegateInfo.Parameters.Select(
                        (p, i) =>
                            $"{p.TypeInfo.FullyQualifiedType} arg{i}{(p.IsOptional ? " = default" : "")}"
                    )
                )
            );

            signatureBuilder.Append(") => throw null!)");

            var handlerSignature = signatureBuilder.ToString();

            return handlerSignature;
        }
    }
}

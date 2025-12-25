// Portions of this file are derived from aspnetcore
// Source:
// https://github.com/dotnet/aspnetcore/blob/v10.0.0/src/Mvc/Mvc.Testing/src/DeferredHostBuilder.cs
// Copyright (c) .NET Foundation and Contributors
// Licensed under the MIT License
// See THIRD-PARTY-LICENSES.txt file in the project root or visit
// https://github.com/dotnet/aspnetcore/blob/v10.0.0/LICENSE.txt

// ReSharper disable InconsistentNaming

namespace MinimalLambda.SourceGenerators.WellKnownTypes;

internal static class WellKnownTypeData
{
    public enum WellKnownType
    {
        Microsoft_Extensions_Primitives_StringValues,
        System_Threading_CancellationToken,
        System_Security_Claims_ClaimsPrincipal,
        System_DateOnly,
        System_DateTimeOffset,
        System_IO_Stream,
        System_IO_Pipelines_PipeReader,
        System_IFormatProvider,
        System_Uri,
        System_String,
        System_Guid,
        System_TimeSpan,
        Microsoft_Extensions_Hosting_GenericHostWebHostBuilderExtensions,
        Microsoft_Extensions_Hosting_HostingHostBuilderExtensions,
        System_Delegate,
        System_Threading_Tasks_Task,
        System_Threading_Tasks_Task_T,
        System_Threading_Tasks_ValueTask,
        System_Threading_Tasks_ValueTask_T,
        System_Reflection_ParameterInfo,
        System_IParsable_T,
        Microsoft_Extensions_DependencyInjection_OutputCacheConventionBuilderExtensions,
        Microsoft_Extensions_DependencyInjection_PolicyServiceCollectionExtensions,
        Microsoft_Extensions_DependencyInjection_FromKeyedServicesAttribute,
        Microsoft_Extensions_DependencyInjection_IServiceCollection,
        System_AttributeUsageAttribute,
        Amazon_DynamoDBv2_Model_AttributeValue,
        System_Collections_Generic_Dictionary_2,
    }

    public static readonly string[] WellKnownTypeNames =
    [
        "Microsoft.Extensions.Primitives.StringValues",
        "System.Threading.CancellationToken",
        "System.Security.Claims.ClaimsPrincipal",
        "System.DateOnly",
        "System.DateTimeOffset",
        "System.IO.Stream",
        "System.IO.Pipelines.PipeReader",
        "System.IFormatProvider",
        "System.Uri",
        "System.String",
        "System.Guid",
        "System.TimeSpan",
        "Microsoft.Extensions.Hosting.GenericHostWebHostBuilderExtensions",
        "Microsoft.Extensions.Hosting.HostingHostBuilderExtensions",
        "System.Delegate",
        "System.Threading.Tasks.Task",
        "System.Threading.Tasks.Task`1",
        "System.Threading.Tasks.ValueTask",
        "System.Threading.Tasks.ValueTask`1",
        "System.Reflection.ParameterInfo",
        "System.IParsable`1",
        "Microsoft.Extensions.DependencyInjection.OutputCacheConventionBuilderExtensions",
        "Microsoft.Extensions.DependencyInjection.PolicyServiceCollectionExtensions",
        "Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute",
        "Microsoft.Extensions.DependencyInjection.IServiceCollection",
        "System.AttributeUsageAttribute",
        "Amazon.DynamoDBv2.Model.AttributeValue",
        "System.Collections.Generic.Dictionary`2",
    ];
}

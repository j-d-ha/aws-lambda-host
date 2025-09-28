using System.Runtime.CompilerServices;

namespace Lambda.Host.Tests.SourceGenerators;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init() => VerifySourceGenerators.Initialize();
}

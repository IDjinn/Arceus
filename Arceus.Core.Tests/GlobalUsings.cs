global using Xunit;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

public static class ModuleInitializer
{

    [ModuleInitializer]
    public static void Init()
    {
        VerifierSettings.AddExtraSettings(_ => _.DefaultValueHandling = DefaultValueHandling.Include);
    }
}
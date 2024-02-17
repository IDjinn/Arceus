global using Xunit;
using System.Runtime.CompilerServices;

public static class ModuleInitializer
{

    [ModuleInitializer]
    public static void Init()
    {
        VerifierSettings.AddExtraSettings(_ => _.DefaultValueHandling = Argon.DefaultValueHandling.Include);
    }
}
using System.Data;
using ArceusCore.Utils.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ArceusCore;


public static class DependencyInjector
{
    public static IServiceCollection AddArceus(this IServiceCollection services, Func<IDbConnection> databaseConnection)
    {
        services.AddTransient<ArceusConnector>(serviceProvider => ActivatorUtilities.CreateInstance<ArceusConnector>(serviceProvider, databaseConnection));
        services.AddSingleton<ReflectionCache>();
        services.AddTransient<Arceus>(static serviceProvider => serviceProvider.GetRequiredService<ArceusConnector>().Connect());
        return services;
    }
}
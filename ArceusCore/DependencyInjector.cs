using System.Data;
using ArceusCore.Utils.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ArceusCore;


public static class DependencyInjector
{
    public static IServiceCollection AddArceus(this IServiceCollection services, Func<IDbConnection> databaseConnection)
    {
        services.AddSingleton<ArceusConnector>(serviceProvider => ActivatorUtilities.CreateInstance<ArceusConnector>(serviceProvider, databaseConnection));
        services.AddSingleton<ReflectionCache>();
        services.AddScoped<Arceus>(serviceProvider => ActivatorUtilities.CreateInstance<Arceus>(serviceProvider, serviceProvider.GetRequiredService<ArceusConnector>().GetConnection()));
        return services;
    }
}
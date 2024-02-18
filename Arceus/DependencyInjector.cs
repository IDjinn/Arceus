using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Arceus;


public static class DependencyInjector
{

    public static IServiceCollection AddArceus(this IServiceCollection services, Func<IDbConnection> databaseConnection)
    {
        services.AddSingleton<ArceusCore>(_ => new ArceusCore(databaseConnection));

        services.AddScoped<Arceus>(serviceProvider => ActivatorUtilities.CreateInstance<Arceus>(serviceProvider, serviceProvider.GetRequiredService<ArceusCore>().GetConnection()));
        
        return services;
    }
}
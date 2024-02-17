using Microsoft.Extensions.DependencyInjection;

namespace Arceus;


public static class DependencyInjector
{

    public static IServiceCollection AddArceus(this IServiceCollection services, string connectionString)
    {

        return services;
    }
}
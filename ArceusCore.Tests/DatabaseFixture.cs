using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;

using Microsoft.Extensions.Logging;
namespace ArceusCore.Tests;

public class DatabaseFixture : IAsyncDisposable
{
    public readonly Arceus Database;
    public DatabaseFixture()
    {
        var servicesCollection = new ServiceCollection()
            .AddLogging(builder =>builder.AddConsole())
            .AddArceus(() => new MySqlConnection("Server=127.0.0.1;Port=3306;Database=arc;Uid=root;Pwd=;AllowUserVariables=true;"));
        
        var serviceProvider = servicesCollection.BuildServiceProvider();
        Database = serviceProvider.GetRequiredService<Arceus>();
    }
    public async ValueTask DisposeAsync()
    {
        await Database.DisposeAsync();
    }
}
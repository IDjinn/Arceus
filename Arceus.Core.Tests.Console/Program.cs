using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace Arceus.Core.Tests.Console;


public static class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddSingleton<CmdTest>()
                    .AddArceus(() => new MySqlConnection(
                            "Server=127.0.0.1;Port=3306;Database=arc;Uid=root;Pwd=;AllowUserVariables=true;"));
            })
            .ConfigureLogging(logger =>
            {
                logger.AddConsole();
            });
        
        var host = builder.Build();
        
        host.Services.GetRequiredService<CmdTest>();
        host.Run();
    }
}
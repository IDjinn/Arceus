using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Arceus;
using Arceus;
using Arceus.Database;
using Arceus.Utils.Parsers;

namespace Arceus.Tests.Console;


public static class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddSingleton<CmdTest>()
                    .AddArceus(() =>
                       (IDbConnection) new MySqlConnection(
                            "Server=127.0.0.1;Port=3306;Database=arc;Uid=root;Pwd=;AllowUserVariables=true;"));
                
                
                //.AddTransient<IDbConnection>(_ => new MySqlConnection("Server=127.0.0.1;Port=3306;Database=arc;Uid=root;Pwd=;AllowUserVariables=true;"))
                //.AddArceus("Server=127.0.0.1;Port=3306;Database=arc;Uid=root;Pwd=;AllowUserVariables=true;");
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
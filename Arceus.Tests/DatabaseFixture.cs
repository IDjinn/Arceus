using System.Data;
using MySql.Data.MySqlClient;

namespace Arceus.Tests;

public class DatabaseFixture : IAsyncDisposable
{
    private readonly MySqlConnection _db;
    public IDbConnection Database => _db;
    public DatabaseFixture()
    {
        _db = new MySqlConnection("Server=127.0.0.1;Port=3306;Database=arc;Uid=root;Pwd=;AllowUserVariables=true;");
    }
    public async ValueTask DisposeAsync()
    {
        await _db.DisposeAsync();
    }
}
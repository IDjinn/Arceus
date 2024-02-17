using System.Data;
using Arceus.Database;
using Arceus.Tests.Console.Entities;

namespace Arceus.Tests.Console;

public class CmdTest
{
    private IDbConnection _dbConnection;
    public CmdTest(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;

        Try();
    }

    private void Try()
    {
       var test = _dbConnection.Query<Item>("SELECT * FROM `items_base` WHERE `id` = @idMax",new ()
       {
           {"@idMax", 14}
       });
       
       
    }
}
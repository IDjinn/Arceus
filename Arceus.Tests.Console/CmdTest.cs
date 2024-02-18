using System.Data;
using Arceus.Database;
using Arceus.Tests.Console.Entities;

namespace Arceus.Tests.Console;

public class CmdTest
{
    private Arceus _arceus;
    public CmdTest(Arceus arceus)
    {
        _arceus = arceus;

        Try();
    }

    private void Try()
    {
        var str = "UPDATE `users` SET `auth_ticket` = 'test' WHERE `id` = @Id";
       var test = _arceus.Query<Item>(str,new 
       {
           @Id= 1
       }
       );


       _arceus.Commit();
       _arceus.Rollback();
    }
}
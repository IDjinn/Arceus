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
        var userFake = new User() { Id = 1, AuthTicket = "fake-user", IsOnline = false};
        _arceus.Query<User>($@"
                             UPDATE `users` SET 
                                                `auth_ticket` = @{nameof(User.AuthTicket)}, 
                                                `online` = @{nameof(User.IsOnline)} 
                             WHERE `id` = @Id ",userFake);
                             
       _arceus.Commit();
    }
}
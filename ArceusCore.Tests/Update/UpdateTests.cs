using ArceusCore.Tests.Entities;

namespace ArceusCore.Tests.Update;

[UsesVerify]
public class UpdateTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public UpdateTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task test_update_single_row_with_entity_parameter()
    {

        var userFake = new User() { Id = 1, Sso = Guid.NewGuid().ToString(), IsOnline = false};
        var updatedRows = _fixture.Database.NonQuery($@"
                             UPDATE `users` SET 
                                                `auth_ticket` = @{nameof(User.Sso)}, 
                                                `online` = @{nameof(User.IsOnline)} 
                             WHERE `id` = @Id ",userFake);
        
        Assert.Equal(1,updatedRows);
    }
}
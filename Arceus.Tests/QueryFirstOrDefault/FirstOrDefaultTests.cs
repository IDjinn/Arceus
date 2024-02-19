using Arceus.Database;
using Arceus.Tests.Entities;

namespace Arceus.Tests;

[UsesVerify]
public class FirstOrDefaultTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public FirstOrDefaultTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task query_first_or_default()
    {
        var room = _fixture.Database.QueryFirstOrDefault<Room>("SELECT * FROM `rooms` WHERE `id` = @Id LIMIT 1", new { Id = 50 });
        Assert.NotNull(room);
        Assert.Equal(RoomState.Open, room!.State);
        await Verify(room);
    }
}
using ArceusCore.Tests.Entities;

namespace ArceusCore.Tests;

[UsesVerify]
public class QueryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public QueryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task query_multiple_rows()
    {
        var rooms = _fixture.Database.Query<Room>("SELECT * FROM `rooms`");
        Assert.NotNull(rooms);
        Assert.NotEmpty(rooms);
        Assert.Equal(RoomState.Open, rooms.ToList()[0].State);
        await Verify(rooms);
    }
    
    
    [Fact]
    public async Task query_multiple_rows_with_parameters()
    {
        var room = _fixture.Database.Query<Room>("SELECT * FROM `rooms` WHERE `id` = @Id", new { @Id = 50 });
        Assert.NotNull(room);
        Assert.NotEmpty(room);
        Assert.Equal(RoomState.Open, room.ToList()[0].State);
        await Verify(room);
    }
}
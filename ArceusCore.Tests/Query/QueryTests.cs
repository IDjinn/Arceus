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
        var rooms = (await _fixture.Database.Query<Room>("SELECT * FROM `rooms`")).ToList();
        Assert.NotEmpty( rooms);
        Assert.Equal(RoomState.Open, rooms.ToList()[0].State);
        await Verify(rooms);
    }


    [Fact]
    public async Task query_multiple_rows_with_parameters()
    {
        var room =await  _fixture.Database.QueryFirstOrDefault<Room>(new("SELECT * FROM `rooms` WHERE `id` = @Id", [new { @Id = 50 }]));
        Assert.NotNull(room);
        Assert.Equal(RoomState.Open, room!.State);
        await Verify(room);
    }

    // [Fact]
    // public async Task query_sql_version()
    // {
    //     var result =await ( await _fixture.Database.Query<object>("SELECT VERSION()")).ToListAsync();
    //     Assert.NotEmpty(result);
    //     await Verify(result);
    // }
}
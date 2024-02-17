using Arceus.Database;
using Arceus.Tests.Entities;
using Argon;

namespace Arceus.Tests;

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
        var queryResult = _fixture.Database.Query<Room>("SELECT * FROM `rooms`");
        Assert.NotNull(queryResult);
        Assert.NotEmpty(queryResult.Data);
        Assert.Equal(RoomState.Open, queryResult.Data.ToList()[0].State);
        await Verify(queryResult.Data);
    }
    
    
    [Fact]
    public async Task query_multiple_rows_with_parameters()
    {
        var queryResult = _fixture.Database.Query<Room>("SELECT * FROM `rooms` WHERE `id` = @Id", new { @Id = 50 });
        Assert.NotNull(queryResult);
        Assert.NotEmpty(queryResult.Data);
        Assert.Equal(RoomState.Open, queryResult.Data.ToList()[0].State);
        await Verify(queryResult.Data);
    }
}
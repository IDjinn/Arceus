using ArceusCore.Utils;

namespace ArceusCore.Tests.Insert;

public class InsertTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public InsertTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task insert_single_row()
    {
        var inserted = await _fixture.Database.InsertQuery(new Query(
            "INSERT INTO `user_items` (user_id, item_id, is_gifted, limited_sells, limited_stack) VALUES " +
            "(1,1,'0',0,0)",[]
        ));

        Assert.True(inserted > 0);
    }
}
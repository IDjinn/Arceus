namespace ArceusCore.Tests.UnitOfWork;

[UsesVerify]
public class DisposeTests 
{

    [Fact]
    public async Task test_dispose_async()
    {
        await using var arceus = new DatabaseFixture().Database;
        var something = arceus.Query<string>("SELECT VERSION()");
        Assert.NotNull(something);
        await arceus.DisposeAsync();
    }
    
    
    [Fact]
    public void test_dispose()
    { 
        using var arceus = new DatabaseFixture().Database;
        var something = arceus.Query<string>("SELECT VERSION()");
        Assert.NotNull(something);
        arceus.Dispose();
    }
}
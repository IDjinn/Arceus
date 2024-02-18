using System.Data;
using Arceus.Database;
using Arceus.Tests.Entities;

namespace Arceus.Tests.Injection;

// https://www.invicti.com/blog/web-security/sql-injection-cheat-sheet/
public class SQLInjectionTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public SQLInjectionTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task inject_sql_with_comment()
    {
        Assert.Throws<SqlInjectionException>(() => _fixture.Database.Query<User>(
            "SELECT * FROM `users` WHERE `auth_ticket`= 'password'--\n\nSELECT VERSION()"));
    }

    [Fact]
    public async Task inject_sql_with_comment_or_one_equals_one()
    {
        Assert.Throws<SqlInjectionException>(() => _fixture.Database.Query<User>(
            "SELECT * FROM `users` WHERE `auth_ticket`= 'password'-- OR 1 = 1"));
    }
    
    
    [Fact]
    public async Task inject_sql_with_comment_hashtag()
    {
        Assert.Throws<SqlInjectionException>(() => _fixture.Database.Query<User>(
            "SELECT * FROM `users` WHERE `auth_ticket`= 'password'#\n\nSELECT VERSION()"));
    }
    
    
    [Fact]
    public async Task inject_with_open_apostrophe()
    {
        Assert.Throws<SqlInjectionException>(() => _fixture.Database.Query<User>(
            "SELECT * FROM `users` WHERE `auth_ticket`= 'password"));
    }
    
    
    [Fact]
    public async Task inject_drop_table_inline_comments()
    {
        Assert.ThrowsAny<Exception>(() => _fixture.Database.Query<User>(
            "DROP/*comment*/ users"));
    }
    
    [Fact]
    public async Task inject_stack_queries()
    {
        Assert.Throws<SqlInjectionException>(() => _fixture.Database.Query<User>(
            "SELECT * FROM users; DROP users--\n"));
    }
    
    
    [Fact]
    public async Task inject_if_statement()
    {
        Assert.Throws<SqlInjectionException>(() => _fixture.Database.Query<User>(
            "SELECT * FROM users WHERE `auth_ticket` = 'password' OR IF(1=1,'true','false')\n"));
    }
    
    
    [Fact]
    public async Task inject_union_query()
    {
        Assert.Throws<SqlInjectionException>(() => _fixture.Database.Query<User>(
            "SELECT * FROM users WHERE `auth_ticket` = 'password' UNION SELECT 1"));
    }
    
    
    [Theory]
    [InlineData("SELECT * FROM users WHERE `username` = 'admin' OR IF(1=1,'true','false')")]
    [InlineData("SELECT * FROM users WHERE `username` = 'admin' OR 1=1")]
    [InlineData("SELECT * FROM users WHERE `username` = 'admin' AND 1=1")]
    [InlineData("SELECT * FROM users WHERE username = 'admin' --")]
    [InlineData("SELECT * FROM users WHERE username = 'admin' #")]
    [InlineData("SELECT * FROM users WHERE username = 'admin' /*")]
    [InlineData("SELECT * FROM users WHERE username = 'admin' or 1=1--")]
    [InlineData("SELECT * FROM users WHERE username = 'admin' or 1=1#")]
    [InlineData("SELECT * FROM users WHERE username = 'admin' or 1=1/*")]
    [InlineData("SELECT * FROM users WHERE username = 'admin') or '1'='1--")]
    [InlineData("SELECT * FROM users WHERE username = 'admin') or ('1'='1--")]
    [InlineData("SELECT * FROM users WHERE username = 'admin' UNION SELECT 1, 'anotheruser', 'doesnt matter', 1--")]
    [InlineData("SELECT * FROM users WHERE username = 'admin' AND 1=0 UNION ALL SELECT 'admin', '81dc9bdb52d04dc20036dbd8313ed055'")]
    [InlineData("SELECT * FROM users WHERE username = 'admin' HAVING 1=1 --")]
    [InlineData("SELECT * FROM users WHERE username = 'admin' ORDER BY 1--")]
    [InlineData("SELECT * FROM users WHERE username = 'admin'; insert into users values( 1, 'hax0r', 'coolpass', 9 )/*")]
    public async Task inject_bypass_login(string query)
    {
        Assert.Throws<SqlInjectionException>(() => _fixture.Database.Query<User>(query));
    }
    
    [Theory]
    [InlineData("SELECT table_name FROM information_schema.tables WHERE table_schema = 'databasename'\n\n")]
    public async Task inject_query_column_names(string query)
    {
        Assert.Throws<SqlInjectionException>(() => _fixture.Database.Query<User>(query));
    }
    [Theory]
    [InlineData("FALSE : SELECT * FROM `users`")]
    public async Task inject_false_statement(string query)
    {
        Assert.Throws<SqlInjectionException>(() => _fixture.Database.Query<User>(query));
    }
}

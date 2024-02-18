using Arceus.Database.Attributtes;

namespace Arceus.Tests.Entities;

[Table("users")]
public class User
{
    [Column("id")]
    public int Id { get; set; }
    
    [Column("auth_ticket")]
    public string Sso { get; set; } 
}
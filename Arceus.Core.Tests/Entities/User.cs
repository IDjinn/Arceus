using Arceus.Core.Database.Attributtes;
using Arceus.Core.Utils.Parsers;

namespace Arceus.Core.Tests.Entities;

[Table("users")]
public class User
{
    [Column("id")]
    public int Id { get; set; }
    
    [Column("auth_ticket")]
    public string Sso { get; set; } 
    
    [Column("online")]
    [Converter(typeof(EnumToBoolConverter))]
    public bool IsOnline { get; set; } 
}
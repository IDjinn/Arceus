using Arceus.Database.Attributtes;
using Arceus.Utils.Parsers;

namespace Arceus.Tests.Console.Entities;

[Table("users")]
public class User
{
    [Column("id")]
    public uint Id { get; init; }
    
    [Column("auth_ticket")]
    public string AuthTicket  { get; init; }
    
    [Column("online")]
    [Converter(typeof(EnumToBoolConverter))]
    public bool IsOnline { get; set; }
}
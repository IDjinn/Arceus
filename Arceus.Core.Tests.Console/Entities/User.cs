using Arceus.Core.Database.Attributtes;
using Arceus.Core.Utils.Parsers;

namespace Arceus.Core.Tests.Console.Entities;

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
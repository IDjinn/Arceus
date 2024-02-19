using ArceusCore.Database.Attributtes;
using ArceusCore.Utils.Parsers;

namespace ArceusCore.Tests.Console.Entities;

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
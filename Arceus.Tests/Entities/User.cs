﻿using Arceus.Database.Attributtes;
using Arceus.Utils.Parsers;

namespace Arceus.Tests.Entities;

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
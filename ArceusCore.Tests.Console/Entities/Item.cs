using ArceusCore.Database.Attributes;

namespace ArceusCore.Tests.Console.Entities;

[Table("items_base")]
public class Item
{
    [Column("id")] 
    public uint Id { get; init; } 
    
    [Column("public_name")] 
    public string CatalogName { get; init; } 
    
    [Column("type")] 
    [Converter(typeof(ItemTypeConverter))] 
    public ItemType Type { get; init; } 
}

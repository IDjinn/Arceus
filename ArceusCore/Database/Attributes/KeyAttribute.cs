namespace ArceusCore.Database.Attributes;

[Flags]
public enum KeyType : byte
{
    AutoIncremental,
    PrimaryKey
    
}

public class KeyAttribute : Attribute
{
    public KeyType Type { get; init; }
}
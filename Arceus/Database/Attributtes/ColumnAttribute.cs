namespace Arceus.Database.Attributtes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Delegate, AllowMultiple = false)]
public class ColumnAttribute : Attribute
{
    public ColumnAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
}
// ReSharper disable ClassNeverInstantiated.Global
namespace ArceusCore.Database.Attributtes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Delegate, AllowMultiple = false)]
public class ColumnAttribute(string name) : Attribute
{
    public string Name { get; set; } = name;
}
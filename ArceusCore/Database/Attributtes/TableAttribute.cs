// ReSharper disable ClassNeverInstantiated.Global
namespace ArceusCore.Database.Attributtes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false)]
public class TableAttribute(string name) : Attribute
{
    public string Name { get; set; } = name;
}
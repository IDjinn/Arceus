using ArceusCore.Utils.Interfaces;
// ReSharper disable ClassNeverInstantiated.Global

namespace ArceusCore.Database.Attributtes;

public class ConverterAttribute : Attribute
{
    public Type Type { get; init; }
    public ConverterAttribute(Type type)
    {
        Type = type;
        if (!type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConvertible<,>)))
            throw new InvalidOperationException($"Converter must be an instance of {nameof(IConvertible)}<TSource, TValue>");
    }
}
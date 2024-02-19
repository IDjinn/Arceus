using System.Collections.Concurrent;
using System.Reflection;

namespace Arceus.Core.Utils.Reflection;

public class ReflectionCache
{
    private readonly IDictionary<Type, object> _types = new ConcurrentDictionary<Type, object>();

    private readonly IDictionary<object, IDictionary<string, MethodInfo>> _methods =
        new ConcurrentDictionary<object, IDictionary<string, MethodInfo>>();

    private readonly Dictionary<Type, IDictionary<string, IDictionary<Type, Attribute>>> _attributes = new();
    private readonly IDictionary<Type, IDictionary<string, PropertyInfo>> _properties = new Dictionary<Type, IDictionary<string, PropertyInfo>>();



    public PropertyInfo GetPropertyInfo(Type type, string propertyName)
    {
        if (_properties.ContainsKey(type) && _properties[type].TryGetValue(propertyName, out var value))
            return value;

        var properties = GetPropertiesOf(type);
        return properties[propertyName];
    }
    public IDictionary<string, PropertyInfo> GetPropertiesOf(Type type)
    {
        if (_properties.ContainsKey(type))
            return _properties[type];

        _properties.Add(type, new Dictionary<string, PropertyInfo>());
        foreach (var propertyInfo in type.GetProperties())
        {
            _properties[type].Add(propertyInfo.Name, propertyInfo);
        }

        return _properties[type];
    }

    public IDictionary<string, IDictionary<Type, Attribute>> GetPropertiesAttributes(Type type)
    {
        if (!_attributes.ContainsKey(type))
        {
            _attributes[type] = new Dictionary<string, IDictionary<Type, Attribute>>();
            var properties = GetPropertiesOf(type);
            foreach (var (propertyName, propertyInfo) in properties)
            {
                if (!_attributes[type].ContainsKey(propertyName))
                    _attributes[type].Add(propertyName, new Dictionary<Type, Attribute>());

                foreach (var customAttribute in propertyInfo.GetCustomAttributes())
                {
                    _attributes[type][propertyName].Add(customAttribute.GetType(), customAttribute);
                }
            }
        }

        return _attributes[type];
    }

    public object GetInstance(Type type)
    {
        if (_types.ContainsKey(type))
            return _types[type];

        var instance = Activator.CreateInstance(type)!;
        _types.Add(type, instance);
        return instance;
    }

    public MethodInfo GetMethod(object instance, string methodName)
    {
        if (_methods.ContainsKey(instance))
            return _methods[instance][methodName];

        var methods = instance.GetType().GetTypeInfo().DeclaredMethods.ToArray();
        _methods.Add(instance, methods.ToDictionary(m => m.Name, m => m));
        return methods.FirstOrDefault(m => m.Name == methodName) ?? throw new InvalidOperationException("Method " + methodName + " doesn't exists on type " + instance.GetType().Name);
    }
}
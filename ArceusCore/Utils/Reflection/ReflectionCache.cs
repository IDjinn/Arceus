using System.Collections.Concurrent;
using System.Reflection;

namespace ArceusCore.Utils.Reflection;

public class ReflectionCache
{
    private readonly ConcurrentDictionary<Type, object> _types = new();
    private readonly ConcurrentDictionary<object, IDictionary<string, MethodInfo>> _methods = new();
    private readonly ConcurrentDictionary<Type, IDictionary<string, IDictionary<Type, Attribute>>> _attributes = new();
    private readonly ConcurrentDictionary<Type, IDictionary<string, PropertyInfo>> _properties = new();

    public PropertyInfo GetPropertyInfo(Type type, string propertyName)
    {
        if (_properties.ContainsKey(type) && _properties[type].TryGetValue(propertyName, out var value))
            return value;

        var properties = GetPropertiesOf(type);
        return properties[propertyName];
    }
    public IDictionary<string, PropertyInfo> GetPropertiesOf(Type type)
    {
        if (_properties.TryGetValue(type, out var properties))
            return properties;

        _properties.TryAdd(type, new ConcurrentDictionary<string, PropertyInfo>());
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
            _attributes[type] = new ConcurrentDictionary<string, IDictionary<Type, Attribute>>();
            var properties = GetPropertiesOf(type);
            foreach (var (propertyName, propertyInfo) in properties)
            {
                if (!_attributes[type].ContainsKey(propertyName))
                    _attributes[type].Add(propertyName, new ConcurrentDictionary<Type, Attribute>());

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
        if (_types.TryGetValue(type, out var instance))
            return instance;

        instance = Activator.CreateInstance(type)!;
        _types.TryAdd(type, instance);
        return instance;
    }

    public MethodInfo GetMethod(object instance, string methodName)
    {
        if (_methods.ContainsKey(instance))
            return _methods[instance][methodName];

        var methods = instance.GetType().GetTypeInfo().DeclaredMethods.ToArray();
        _methods.TryAdd(instance, methods.ToDictionary(static m => m.Name, m => m));
        return methods.FirstOrDefault(m => m.Name == methodName) ?? throw new InvalidOperationException("Method " + methodName + " doesn't exists on type " + instance.GetType().Name);
    }
}
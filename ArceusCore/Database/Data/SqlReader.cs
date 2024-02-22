// #define __CONVERT_TYPE_WITH_RUNTIME__
using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;
using ArceusCore.Database.Attributtes;
using ArceusCore.Utils;
using ArceusCore.Utils.Interfaces;
using ArceusCore.Utils.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ArceusCore.Database.Data;


public class InvalidConversionException(string message, object? value, object targetProperty, Exception exception) : Exception(message)
{
    public object? Value { get; init; } = value;
    public object TargetProperty { get; init; } = targetProperty;
    public Exception Exception { get; init;} = exception;
}

public class SqlReader<TResult> : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Query _query;
    private readonly Record<TResult>? _record;
    private readonly IDataReader _reader;
    private readonly Table<TResult> _table;
    private readonly ReflectionCache _cache;

    public ICollection<TResult> Data
    {
        get
        {
            if (_table._data.Count > 0)
                return _table._data;

            return _table._originalRows[0]._databaseValues.Cast<TResult>().ToList();
        }
    }
    public Table<TResult> Table => _table;

    public SqlReader(
        IDataReader reader, 
        IServiceProvider serviceProvider,
        Query query,
        Record<TResult>? record
        )
    {
        _reader = reader;
        _serviceProvider = serviceProvider;
        _query = query;
        _record = record;
        _cache = serviceProvider.GetRequiredService<ReflectionCache>();
        _table = new Table<TResult>(reader.FieldCount);

        for (var i = 0; i < reader.FieldCount; i++)
        {
            _table._columns.Add(reader.GetName(i));
        }

        if (typeof(TResult).IsPrimitive 
            || typeof(TResult) == typeof(string)
            || typeof(TResult) == typeof(object))
        {
            while (reader.Read())
            {
                var row = new Row(reader.FieldCount);
                for (var i = 0; i < reader.FieldCount; i++)
                    row._databaseValues.Add(reader.GetValue(i));

                _table._originalRows.Add(row);
            }
        }

        if (typeof(TResult).GetCustomAttribute<TableAttribute>() is not { } tableAttribute) return;
        
        var propertiesWithColumn = _cache.GetPropertiesAttributes(typeof(TResult));
        var index = 0;
        while (reader.Read())
        {
            var row = new Row(reader.FieldCount);
            for (var i = 0; i < reader.FieldCount; i++)
                row._databaseValues.Add(reader.GetValue(i));

            _table._originalRows.Add(row);
            var data = CreateInstanceOf<TResult>();
            foreach (var (propertyName, attributes) in propertiesWithColumn)
            {
                // if we dont have column attribute we just skip this property.
                if (!attributes.TryGetValue(typeof(ColumnAttribute), out var foundAttribute) ||
                    foundAttribute is not ColumnAttribute columnAttribute)
                    continue;

                var dbValue = _table[index, columnAttribute.Name];
                var propertyInfo = _cache.GetPropertyInfo(typeof(TResult), propertyName);
                var value = dbValue.Object?.GetType() == typeof(DBNull) ? null : dbValue.Object;
                try
                {
                    foreach (var (_, attribute) in attributes)
                    {
                        if (attribute is not ConverterAttribute converterAttribute) 
                            continue;
                        
                        var instance = _cache.GetInstance(converterAttribute.Type);
                        var method = _cache.GetMethod(instance, nameof(IConvertible<string, string>.Parse));
                        if (!dbValue.HasValue) // TODO: if dbvalue is DBNull maybe throw exception
                            throw new InvalidOperationException(nameof(dbValue));

                        value = method.Invoke(instance, [dbValue.Object]);
                    }

                    propertyInfo.SetValue(data, value);
                }
                catch (ArgumentException argumentException)
                {
#if __CONVERT_TYPE_WITH_RUNTIME__
                        try
                        {
                            value =
 Convert.ChangeType(value, propertyInfo.PropertyType); // this method is very expensive
                        }
                        catch
                        {
                            throw new InvalidConversionException(
                                $"Could not convert '{value}' to property '{propertyName}' of '{data}'. Expected type is '{propertyInfo.PropertyType}' got {value?.GetType()}",
                                data, propertyName, argumentException);
                        }
#else
                    throw new InvalidConversionException(
                        $"Could not convert '{value}' to property '{propertyName}' of '{data}'. Expected type is '{propertyInfo.PropertyType}' got {value?.GetType()}",
                        data, propertyName, argumentException);
#endif
                }
                catch (TargetException targetException)
                {
                    throw new InvalidConversionException(
                        $"Could not convert '{data}' to property '{propertyName}'. Expected type is '{propertyInfo.PropertyType}' got {value?.GetType()}",
                        data, propertyName, targetException);
                }
                catch (Exception exception)
                {
                    throw;
                }
            }

            _table._data.Add(data);
            index++;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private T CreateInstanceOf<T>()
    {
        try
        {
            var instanceType = _serviceProvider.GetService<T>();
            if (_record.HasValue)
                return (T)ActivatorUtilities.CreateInstance(_serviceProvider,instanceType!.GetType(), _record.Value.AdditionalParameters);
            return (T)ActivatorUtilities.CreateInstance(_serviceProvider,instanceType!.GetType());
        }
        catch
        {
            // ignored
        }

        try // handles interfaces type or instance registered in DI container
        {
            return (T)_serviceProvider.GetRequiredService(typeof(T));
        }
        catch // otherwise, just the class instance
        {
            return Activator.CreateInstance<T>();
        }
    }

    public void Dispose()
    {
        _reader.Dispose();
    }
}
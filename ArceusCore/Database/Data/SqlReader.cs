// #define __CONVERT_TYPE_WITH_RUNTIME__

using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Runtime.CompilerServices;
using ArceusCore.Database.Attributes;
using ArceusCore.Utils;
using ArceusCore.Utils.Interfaces;
using ArceusCore.Utils.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ArceusCore.Database.Data;


public class InvalidConversionException(string message, object? value, object targetProperty, Exception exception) : Exception(message)
{
    public object? Value { get; init; } = value;
    public object TargetProperty { get; init; } = targetProperty;
    public Exception Exception { get; init;} = exception;
}

public class SqlReader<TResult> 
    where TResult : new()
{
    private readonly ILogger<SqlReader<TResult>> _logger;
    private readonly DbDataReader _reader;
    private readonly Func<TResult> _factory;
    private readonly Table<TResult> _table;
    private readonly ReflectionCache _cache;

    public ICollection<TResult> Data => new List<TResult>();
    public Table<TResult> Table => _table;

    public SqlReader(IServiceProvider serviceProvider,
        DbDataReader _reader,
        Query query,
        Func<TResult> factory)
    {
        _logger = serviceProvider.GetRequiredService<ILogger<SqlReader<TResult>>>();
        this._reader = _reader;
        _factory = factory;
        _cache = serviceProvider.GetRequiredService<ReflectionCache>();
        _table = new Table<TResult>(100);

        for (var i = 0; i < _reader.FieldCount; i++)
        {
            _table.AddColumn(_reader.GetName(i));
        }
        
        while (_reader.Read())
        {
            var row = new Row(_reader.FieldCount);
            for (var i = 0; i < _reader.FieldCount; i++)
            {
                var value = _reader.GetValue(i);
                row._databaseValues.Add(value);
            }
            _table.AddRow(row);
        }
        
        _reader.Dispose();
    }

    public IEnumerable<TResult> ReadEnumerable()
    {
        if (_table.Rows.Count == 0 || _table.Rows[0]._databaseValues.Count == 0)
            yield break;
        
        if (typeof(TResult).IsPrimitive)
        {
            yield return (TResult)Convert.ChangeType(_table.Rows[0]._databaseValues[0], typeof(TResult));
        }
        
        if (typeof(TResult).GetCustomAttribute<TableAttribute>() is not { } tableAttribute)
            yield break;

        var propertiesWithColumn = _cache.GetPropertiesAttributes(typeof(TResult));
        var columns = _table.Columns.ToList();


        foreach (var row in _table.Rows)
        {
            var data = _factory();
            foreach (var (propertyName, attributes) in propertiesWithColumn)
            {
                // if we dont have column attribute we just skip this property.
                if (!attributes.TryGetValue(typeof(ColumnAttribute), out var foundAttribute) ||
                    foundAttribute is not ColumnAttribute columnAttribute)
                    continue;

                var dbValue = row[columns.IndexOf(columnAttribute.Name)];
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
            }

            _table.AddData(data);
            yield return data;
        }
    }
}
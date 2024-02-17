using System.Data;
using System.Dynamic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Arceus.Database.Attributtes;
using Arceus.Utils.Interfaces;
using Arceus.Utils.Reflection;

namespace Arceus.Database;

public class SqlReader<TResult>
{
    private readonly IDataReader _reader;
    private readonly Table<TResult> _table;

    public IEnumerable<TResult> Data => _table._data;

    public SqlReader(IDataReader reader)
    {
        _reader = reader;
        _table = new Table<TResult>();

        if (typeof(TResult).GetCustomAttribute<TableAttribute>() is { } tableAttribute)
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                _table._columns.Add(reader.GetName(i));
            }

            var propertiesWithColumn = ReflectionCache.GetPropertiesAttributes(typeof(TResult));
            var index = -1;
            while (reader.Read())
            {
                index++;
                var row = new Row();
                for (var i = 0; i < reader.FieldCount; i++)
                    row._databaseValues.Add(reader.GetValue(i));

                _table._originalRows.Add(row);
                var data = Activator.CreateInstance<TResult>();
                foreach (var (propertyName, attributes) in propertiesWithColumn)
                {
                    // if we dont have column attribute we just skip this property.
                    if (!attributes.TryGetValue(typeof(ColumnAttribute), out var foundAttribute) || foundAttribute is not ColumnAttribute columnAttribute)
                        continue;
                    
                    var dbValue =  _table[index, columnAttribute.Name];
                    var value = dbValue.Object;
                    foreach (var (_, attribute) in attributes)
                    {
                        if (attribute is ConverterAttribute converterAttribute)
                        {
                            var instance = ReflectionCache.GetInstance(converterAttribute.Type);
                            var method =
                                ReflectionCache.GetMethod(instance, nameof(IConvertible<string, string>.Parse));
                            if (!dbValue.HasValue)
                                throw new InvalidOperationException(nameof(dbValue));

                            value = method.Invoke(instance, [dbValue.Object]);
                        }
                    }
                    ReflectionCache.GetPropertyInfo(typeof(TResult),propertyName).SetValue(data, value);
                }

                _table._data.Add(data);
            }
        }
    }
}
using System.Data;
using Arceus.Core.Database.Attributtes;
using Arceus.Core.Database.Data;
using Arceus.Core.Utils;
using Arceus.Core.Utils.Interfaces;
using Arceus.Core.Utils.Reflection;
using Microsoft.Extensions.Logging;

namespace Arceus.Core;

public class Arceus : IAsyncDisposable
{
    private const int MaximumFindValueDepth = 15;

    private readonly ILogger<Arceus> _logger;
    private readonly IDbConnection _connection;
    private readonly IDbTransaction _transaction;
    private readonly ReflectionCache _cache;

    public Arceus(ILogger<Arceus> logger, IDbConnection connection, ReflectionCache cache)
    {
        _logger = logger;
        _connection = connection;
        _cache = cache;

        _connection.Open();
        _transaction = _connection.BeginTransaction();
    }

    public void Commit()
    {
        _transaction.Commit();
    }

    public void Rollback()
    {
        _transaction.Rollback();
    }

    public TResult? QueryFirstOrDefault<TResult>(
        Query query,
        object? parameters = null
    )
    {
        return __query_internal<TResult>(query, parameters, CommandBehavior.SingleRow).FirstOrDefault();
    }

    public IEnumerable<TResult> Query<TResult>(
        Query query,
        object? parameters = null
    )
    {
        return __query_internal<TResult>(query, parameters);
    }

    public int NonQueryInternal(
        Query query,
        object? parameters = null
    )
    {
        return __non_query_internal(query, parameters);
    }

    private int __non_query_internal(Query query,
        object? parameters = null,
        CommandBehavior behavior = CommandBehavior.Default)
    {
        using var cmd = _transaction.Connection!.CreateCommand();
        cmd.CommandText = query.Value;
        HandleQueryParameters(parameters, cmd);
        cmd.Prepare();
        return cmd.ExecuteNonQuery();
    }


    private IEnumerable<TResult> __query_internal<TResult>(
        Query query,
        object? parameters = null,
        CommandBehavior behavior = CommandBehavior.Default
    )
    {
        SqlReader<TResult>? reader = null;
        try
        {
            using var cmd = _transaction.Connection!.CreateCommand();
            cmd.CommandText = query.Value;
            HandleQueryParameters(parameters, cmd);
            cmd.Prepare();

            reader = new SqlReader<TResult>(cmd.ExecuteReader(behavior), _cache);
            return reader.Data;
        }
        finally
        {
            reader?.Dispose();
        }
    }
// public ValueTask<IEnumerable<TResult>> QueryAsync<TResult>(
//     Query query,
//     object? parameters = null
// )
// {
//     return default;
// }


// TODO: Implement depth checking & serialization
    private void HandleQueryParameters(object? obj, IDbCommand cmd, int depth = 0)
    {
        if (obj is null) return;
        if (depth++ >= MaximumFindValueDepth) return;

        var objectType = obj.GetType();
        foreach (var (propertyName, propertyInfo) in _cache.GetPropertiesOf(objectType))
        {
            if (propertyInfo.PropertyType.IsClass
                && propertyInfo.PropertyType != typeof(string)
               )
            {
                HandleQueryParameters(propertyInfo.GetValue(obj), cmd);
                continue;
            }

            var parameterByColumnAttribute = cmd.CreateParameter();
            var parameterByPropertyName = cmd.CreateParameter();
            var attributes = propertyInfo.GetCustomAttributes(false);
            if (attributes.OfType<ColumnAttribute>().FirstOrDefault() is { } columnAttribute)
            {
                parameterByColumnAttribute.ParameterName = columnAttribute.Name;
            }
            else
            {
                parameterByColumnAttribute.ParameterName = propertyInfo.Name;
            }

            parameterByPropertyName.ParameterName = propertyInfo.Name;

            if (cmd.Parameters.IndexOf(parameterByColumnAttribute.ParameterName) != -1
                || cmd.Parameters.IndexOf(parameterByPropertyName.ParameterName) != -1)
                continue; // ignore duplicate parameters

            if (attributes.OfType<ConverterAttribute>().FirstOrDefault() is { } converterAttribute)
            {
                var instance = _cache.GetInstance(converterAttribute.Type);
                var method = _cache.GetMethod(instance, nameof(IConvertible<string, string>.Convert));

                var value = method.Invoke(instance, [propertyInfo.GetValue(obj)]);
                parameterByColumnAttribute.Value = value;
                parameterByPropertyName.Value = value;
            }
            else
            {
                var value = propertyInfo.GetValue(obj);
                parameterByColumnAttribute.Value = value;
                parameterByPropertyName.Value = value;
            }
            // var value = FindValue(propertyInfo.GetValue(obj), depth);

            if (!string.IsNullOrEmpty(parameterByColumnAttribute.ParameterName))
                cmd.Parameters.Add(parameterByColumnAttribute);

            if (cmd.Parameters.Contains(parameterByPropertyName.ParameterName))
                continue;

            cmd.Parameters.Add(parameterByPropertyName);
        }
    }

    public async ValueTask DisposeAsync()
    {
        _transaction.Rollback();
        _connection.Close();
        await CastAndDispose(_transaction);
        await CastAndDispose(_connection);

        return;

        static async ValueTask CastAndDispose(IDisposable resource)
        {
            if (resource is IAsyncDisposable resourceAsyncDisposable)
                await resourceAsyncDisposable.DisposeAsync();
            else
                resource.Dispose();
        }
    }
}
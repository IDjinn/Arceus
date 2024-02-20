using System.Data;
using ArceusCore.Database.Attributtes;
using ArceusCore.Database.Data;
using ArceusCore.Utils;
using ArceusCore.Utils.Interfaces;
using ArceusCore.Utils.Reflection;
using Microsoft.Extensions.Logging;

namespace ArceusCore;

public class Arceus : IAsyncDisposable, IDisposable
{
    private const int MaximumFindValueDepth = 15;

    private readonly ILogger<Arceus> _logger;
    private readonly IDbConnection _connection;
    private readonly IDbTransaction _transaction;
    private readonly ReflectionCache _cache;
    private bool _wasCommitted;
    private bool _wasRolledBack;

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
        if (_wasCommitted)
            throw new InvalidOperationException("Transaction was already committed in this scope.");
        
        _transaction.Commit();
        _wasCommitted = true;
    }

    public void Rollback()
    {
        if (_wasRolledBack)
            throw new InvalidOperationException("Transaction was already rolled back in this scope.");
        
        _transaction.Rollback();
        _wasRolledBack = true;
    }

    public TResult QuerySingle<TResult>(
        Query query,
        object? parameters = null
    )
    {
        return __query_internal<TResult>(query, parameters, CommandBehavior.SingleRow).Single();
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

    public int NonQuery(
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
            if (!propertyInfo.PropertyType.IsPrimitive // TODO:check if its class or interface or struct so on
                && propertyInfo.PropertyType != typeof(string)
               )
            {
                HandleQueryParameters(propertyInfo.GetValue(obj), cmd, depth);
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

            if (!string.IsNullOrEmpty(parameterByColumnAttribute.ParameterName))
                cmd.Parameters.Add(parameterByColumnAttribute);

            if (cmd.Parameters.Contains(parameterByPropertyName.ParameterName))
                continue;

            cmd.Parameters.Add(parameterByPropertyName);
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (!_wasCommitted && !_wasRolledBack)
                Rollback();
        
            _connection.Close();
            await CastAndDispose(_transaction);
            await CastAndDispose(_connection);
        }
        finally
        {
            GC.SuppressFinalize(this);
        }
        return;

        static async ValueTask CastAndDispose(IDisposable resource)
        {
            if (resource is IAsyncDisposable resourceAsyncDisposable)
                await resourceAsyncDisposable.DisposeAsync();
            else
                resource.Dispose();
        }
    }

    public void Dispose()
    {
        try
        {
            if (!_wasCommitted && !_wasRolledBack)
                Rollback();
            _transaction.Dispose();
            _connection.Dispose();
        }
        finally
        {
            GC.SuppressFinalize(this);
        }
    }
}
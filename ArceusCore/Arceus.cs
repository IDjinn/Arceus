using System.Data;
using System.Data.Common;
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
    private readonly DbConnection _connection;
    private readonly DbTransaction _transaction;
    private readonly IServiceProvider _serviceProvider;
    private readonly ReflectionCache _cache;
    private bool _wasCommitted;
    private bool _wasRolledBack;

    public Arceus(ILogger<Arceus> logger, DbConnection connection, IServiceProvider serviceProvider, ReflectionCache cache)
    {
        _logger = logger;
        _connection = connection;
        _serviceProvider = serviceProvider;
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

    public async Task<TResult> QuerySingle<TResult>(
        Query query,
        Func<TResult>? factory =null
    )where TResult : new()
    {
        return (await __query_internal(query, factory, CommandBehavior.SingleRow)).Single();
    }
    
    public async ValueTask<TResult?> QueryFirstOrDefault<TResult>(
        Query query,
        Func<TResult>? factory =null
    )where TResult : new()
    {
        return (await __query_internal(query, factory, CommandBehavior.SingleRow)).FirstOrDefault();
    }

    public Task<IEnumerable<TResult>> Query<TResult>(
        Query query,
        Func<TResult>? factory =null
    )where TResult : new()
    {
        return __query_internal(query, factory);
    }

    public Task<int> NonQuery(
        Query query
    )
    {
        return __non_query_internal(query);
    }

    private async Task<int> __non_query_internal(Query query,
        CommandBehavior behavior = CommandBehavior.Default)
    {
        await using var cmd = _transaction.Connection!.CreateCommand();
        cmd.CommandText = query.QueryString;
        HandleQueryParameters(query.Parameters, cmd);
        await cmd.PrepareAsync();
        return await cmd.ExecuteNonQueryAsync();
    }


    public async Task<IEnumerable<TResult>> __query_internal<TResult>(
        Query query,
        Func<TResult>? factory = null,
        CommandBehavior behavior = CommandBehavior.Default
    ) where TResult : new()
    {
        factory ??= ()=> new TResult();
        
        await using var cmd = _transaction.Connection!.CreateCommand();
        cmd.CommandText = query.QueryString;
        HandleQueryParameters(query.Parameters, cmd);
        await cmd.PrepareAsync();

        var reader = await cmd.ExecuteReaderAsync(behavior);
        return new SqlReader<TResult>(_serviceProvider,reader, query, factory)
            .ReadEnumerable();
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

        if (obj is object[] array)
        {
            foreach (var item in array)
            {
                HandleQueryParameters(item, cmd, depth);
            }

            return;
        }

        var objectType = obj.GetType();
        foreach (var (propertyName, propertyInfo) in _cache.GetPropertiesOf(objectType))
        {
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
            else if (!propertyInfo.PropertyType.IsPrimitive // TODO:check if its class or interface or struct so on
                     && propertyInfo.PropertyType != typeof(string)
                    )
            {
                HandleQueryParameters(propertyInfo.GetValue(obj), cmd, depth);
                continue;
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
        
            await _connection.CloseAsync();
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
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;
using ArceusCore.Database.Attributes;
using ArceusCore.Database.Data;
using ArceusCore.Utils;
using ArceusCore.Utils.Interfaces;
using ArceusCore.Utils.Reflection;
using Microsoft.Extensions.Logging;

namespace ArceusCore;

public class Arceus : IAsyncDisposable, IDisposable
{
    private const int MaximumFindValueDepth = 3;
    private readonly Guid _connectionId;

    private readonly ILogger<Arceus> _logger;
    private readonly DbConnection _connection;
    private readonly DbTransaction _transaction;
    private readonly IServiceProvider _serviceProvider;
    private readonly ReflectionCache _cache;
    private bool _wasCommitted;
    private bool _wasRolledBack;

    public Arceus(ILogger<Arceus> logger, DbConnection connection, IServiceProvider serviceProvider, ReflectionCache cache)
    {
        _connectionId = Guid.NewGuid();
        _logger = logger;
        _connection = connection;
        _serviceProvider = serviceProvider;
        _cache = cache;
        
        _connection.Open();
        _transaction = _connection.BeginTransaction();
    }

    public async Task Commit(CancellationToken cancellationToken = default)
    {
        if (_wasRolledBack)
        {
            _logger.LogError("[Connection-{ConnectionId}] - Tried to commit an already rolled back Arceus in current transaction",_connectionId);
            throw new InvalidOperationException("Transaction was already rolled back in this scope.");
        }
        
        if (_wasCommitted)
        {
            _logger.LogError("[Connection-{ConnectionId}] - Tried to commit an already committed Arceus in current transaction", _connectionId);
            throw new InvalidOperationException("Transaction was already committed in this scope.");
        }
        
        await _transaction.CommitAsync(cancellationToken);
        _wasCommitted = true;
        _logger.LogDebug("[Connection-{ConnectionId}] - Committed successfully", _connectionId);
    }

    public async Task Rollback(CancellationToken cancellationToken = default)
    {
        if (_wasRolledBack)
        {
            _logger.LogError("[Connection-{ConnectionId}] - Tried to commit an already committed Arceus in current transaction",_connectionId);
            throw new InvalidOperationException("Transaction was already rolled back in this scope.");
        }
        
        await _transaction.RollbackAsync(cancellationToken);
        _wasRolledBack = true;
        _logger.LogDebug("[Connection-{ConnectionId}] - Rolled back successfully",_connectionId);
    }

    public async Task<TResult> QuerySingle<TResult>(
        Query query,
        Func<TResult>? factory =null,
        CancellationToken cancellationToken = default
    )where TResult : new() // TODO: MAKE IT SCALAR
    {
        return (await __query_internal(query, factory, CommandBehavior.SingleRow, cancellationToken)).Single();
    }
    
    public async ValueTask<TResult?> QueryFirstOrDefault<TResult>(
        Query query,
        Func<TResult>? factory =null,
        CancellationToken cancellationToken = default
    )where TResult : new()
    { // TODO: MAKE IT SCALAR
        return (await __query_internal(query, factory, CommandBehavior.SingleRow, cancellationToken)).FirstOrDefault();
    }

    public Task<IEnumerable<TResult>> Query<TResult>(
        Query query,
        Func<TResult>? factory =null, CancellationToken cancellationToken = default
    )where TResult : new()
    {
        return __query_internal(query, factory, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Executes the command against connection, returning the number of rows affected
    /// </summary>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Affected Rows</returns>
    public Task<int> NonQuery(
        Query query, CancellationToken cancellationToken = default
    )
    {
        return __non_query_internal(query, cancellationToken);
    }

    public Task<int> InsertQuery(
        Query query, CancellationToken cancellationToken = default
    )
    {
        return __insert_query_internal(query, cancellationToken);
    }

    private async Task<int> __insert_query_internal(Query query, CancellationToken cancellationToken = default)
    {
        await using var cmd = _transaction.Connection!.CreateCommand();
        cmd.CommandText = query.QueryString;
        HandleQueryParameters(query.Parameters, cmd);
        await cmd.PrepareAsync(cancellationToken);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
        var insertedId = (await __query_internal<int>("SELECT LAST_INSERT_ID()", cancellationToken: cancellationToken))
            .First();
        return insertedId;
    }

    private async Task<int> __non_query_internal(Query query,
        CancellationToken cancellationToken = default
        )
    {
        await using var cmd = _transaction.Connection!.CreateCommand();
        cmd.CommandText = query.QueryString;
        HandleQueryParameters(query.Parameters, cmd);
        await cmd.PrepareAsync(cancellationToken);
        var affectedRows = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return affectedRows;
    }


    private async Task<IEnumerable<TResult>> __query_internal<TResult>(
        Query query,
        Func<TResult>? factory = null,
        CommandBehavior behavior = CommandBehavior.Default,
        CancellationToken cancellationToken = default
    ) where TResult : new()
    {
        factory ??= ()=> new TResult();
        
        await using var cmd = _transaction.Connection!.CreateCommand();
        cmd.CommandText = query.QueryString;
        HandleQueryParameters(query.Parameters, cmd);
        await cmd.PrepareAsync(cancellationToken);
        var reader = await cmd.ExecuteReaderAsync(behavior, cancellationToken);
        return new SqlReader<TResult>(_serviceProvider,reader, query, factory)
            .ReadEnumerable();
    }


// TODO: Implement depth checking & serialization
    private void HandleQueryParameters(object? obj, IDbCommand cmd, int depth = 0)
    {
        try
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

                if (attributes.OfType<KeyAttribute>().FirstOrDefault() is { } keyAttribute)
                {
                    if (keyAttribute.Type == KeyType.AutoIncremental)
                        continue; // auto increment not need to make as parameter
                } // TODO: EXCEPTIONS FOR THIS
                else if (attributes.OfType<ConverterAttribute>().FirstOrDefault() is { } converterAttribute)
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
        catch (Exception e)
        {
            // ignored
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (!_wasCommitted && !_wasRolledBack)
            {
                _logger.LogDebug("[Connection-{ConnectionId}] - Disposed transaction, was not committed or rolled back", _connectionId);
                await Rollback();
            }
        
            await _connection.CloseAsync();
            await CastAndDispose(_transaction);
            await CastAndDispose(_connection);
        }
        finally
        {
            _logger.LogDebug("[Connection-{ConnectionId}] - Arceus disposed", _connectionId);
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
            {
                _logger.LogDebug("[Connection-{ConnectionId}] - Disposed transaction, was not committed or rolled back",_connectionId);
                _transaction.Rollback();
            }
            _transaction.Dispose();
            _connection.Dispose();
        }
        finally
        {
            _logger.LogDebug("[Connection-{ConnectionId}] - Arceus disposed",_connectionId);
            GC.SuppressFinalize(this);
        }
    }
}
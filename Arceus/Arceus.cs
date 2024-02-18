using System.Data;
using Arceus.Database.Data;
using Arceus.Utils;
using Microsoft.Extensions.Logging;

namespace Arceus;

public class Arceus : IAsyncDisposable
{
    private readonly ILogger<Arceus> _logger;
    private readonly IDbConnection _connection;
    private readonly IDbTransaction _transaction;


    public Arceus(ILogger<Arceus> logger, IDbConnection connection)
    {
        _logger = logger;
        _connection = connection;
        
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
    public IEnumerable<TResult?> Query<TResult>(
        Query query,
        object? parameters = null
    )
    {
        return __query_internal<TResult>(query, parameters);
    }
    
    
    private IEnumerable<TResult?> __query_internal<TResult>(
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

            reader = new SqlReader<TResult>(cmd.ExecuteReader(behavior));
            return reader.Data;
        }
        finally
        {
            reader?.Dispose();
        }
    }
    public ValueTask<IEnumerable<TResult?>> QueryAsync<TResult>(
        Query query,
        object? parameters = null
    )
    {
        return default;
    }
    
    
    private static void HandleQueryParameters(object? parameters, IDbCommand cmd)
    {
        if (parameters is null) return;
        
        var properties = parameters.GetType().GetProperties();
        foreach (var propertyInfo in properties)
        {
            var parameter = cmd.CreateParameter();
            parameter.ParameterName = propertyInfo.Name;
            parameter.Value = propertyInfo.GetValue(parameters);
            cmd.Parameters.Add(parameter);
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
using System.Data;
using Arceus.Database.Data;
using Arceus.Utils;

namespace Arceus.Database;

public static class QueryExtensions
{
    public static ICollection<TReturn> Query<TReturn>(this IDbConnection connection,
        Query query,
        object? parameters = null
        )
    {  
        try
        {
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = query.Value;
            HandleQueryParameters(parameters, cmd);
            cmd.Prepare();
            
            var reader =new SqlReader<TReturn>(cmd.ExecuteReader());
            return reader.Data;
        }
        finally
        {
            connection.Close();
        }
    }
    
    
    public static TReturn? QueryFirstOrDefault<TReturn>(this IDbConnection connection,
        Query query,
        object? parameters = null
    )
    {
        return Query<TReturn>(connection, query, parameters).FirstOrDefault();
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
}
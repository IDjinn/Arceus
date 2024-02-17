using System.Data;
using Arceus.Utils;

namespace Arceus.Database;

public static class QueryExtensions
{
    public static SqlReader<TReturn> Query<TReturn>(
        this IDbConnection connection,
        Query query,
        IDictionary<string, object>? parameters = null
        )
    {
        parameters ??= new Dictionary<string, object>();
        try
        {
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = query.Value;
            foreach (var (key, parameter) in parameters)
            {
                cmd.Parameters[key] = parameter;
            }

            cmd.Prepare();
            return new SqlReader<TReturn>(cmd.ExecuteReader());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        return default;
    }
}
using System.Data;

namespace Arceus.Core;

public class ArceusConnector(Func<IDbConnection> connectionAction)
{
    public IDbConnection GetConnection()
    {
        return connectionAction.Invoke();
    }
}
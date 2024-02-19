using System.Data;

namespace ArceusCore;

public class ArceusConnector(Func<IDbConnection> connectionAction)
{
    public IDbConnection GetConnection()
    {
        return connectionAction.Invoke();
    }
}
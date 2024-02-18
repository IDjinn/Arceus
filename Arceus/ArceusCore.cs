using System.Data;

namespace Arceus;

public class ArceusCore
{
    private readonly Func<IDbConnection> _connectionAction;

    public ArceusCore(Func<IDbConnection> connectionAction)
    {
        _connectionAction = connectionAction;
    }

    public IDbConnection GetConnection()
    {
        return _connectionAction.Invoke();
    }
}
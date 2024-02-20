using System.Data;
using Microsoft.Extensions.DependencyInjection;

namespace ArceusCore;

public class ArceusConnector(
    IServiceProvider provider,
    Func<IDbConnection> connectionAction
    )
{

    public Arceus Connect()
    {
        return ActivatorUtilities.CreateInstance<Arceus>(provider, connectionAction.Invoke());
    }
}
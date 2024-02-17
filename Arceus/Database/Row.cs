using System.Collections.Concurrent;

namespace Arceus.Database;

public class Row
{
    internal List<object> _databaseValues = new();
    internal List<object> _values = new();
    
    public IReadOnlyList<object> DatabaseValues=> _databaseValues.AsReadOnly();

    public Value this[int column]
    {
        get
        {
            if (column < 0 || column >= _databaseValues.Count)
                return default;
            
            return new (_databaseValues[column]);
        }
    }
}
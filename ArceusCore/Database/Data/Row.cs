namespace ArceusCore.Database.Data;

public class Row
{
    internal readonly List<object> _databaseValues ;
    internal readonly List<object> _values;

    public Row(int size)
    {
        _databaseValues = new List<object>(size);
        _values = new List<object>(size);
    }

    public IReadOnlyList<object> DatabaseValues => _databaseValues.AsReadOnly();

    public Value this[int column]
    {
        get
        {
            if (column < 0 || column >= _databaseValues.Count)
                return default;

            return new(_databaseValues[column]);
        }
    }
}
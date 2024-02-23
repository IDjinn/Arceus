namespace ArceusCore.Database.Data;

public class Row
{
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

    internal readonly List<object> _databaseValues;
    internal readonly List<object> _values;

    public Row(int columnsSize)
    {
        _databaseValues = new List<object>(columnsSize);
        _values = new List<object>(columnsSize);
    }

    public IReadOnlyList<object> DatabaseValues
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _databaseValues.AsReadOnly();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public Value this[int column]
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                if (column < 0 || column >= _databaseValues.Count)
                    return default;

                return new Value(_databaseValues[column]);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public void AddColumn(object value)
    {
        _lock.EnterWriteLock();
        try
        {
            _values.Add(value);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void AddValue(object value)
    {
        _lock.EnterWriteLock();
        try
        {
            _values.Add(value);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void AddDatabaseValue(object value)
    {
        _lock.EnterWriteLock();
        try
        {
            _databaseValues.Add(value);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}
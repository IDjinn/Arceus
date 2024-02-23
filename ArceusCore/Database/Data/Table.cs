namespace ArceusCore.Database.Data;


public class Table<T>
{
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

    private readonly List<string> _columns;
    private readonly List<Row> _originalRows;
    private readonly List<T> _data = new();

    public Table(int columnsSize)
    {
        _columns = new List<string>(columnsSize);
        _originalRows = new List<Row>(columnsSize);
    }

    public void AddColumn(string columnName)
    {
        _lock.EnterWriteLock();
        try
        {
            _columns.Add(columnName);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void AddRow(Row row)
    {
        _lock.EnterWriteLock();
        try
        {
            _originalRows.Add(row);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void AddData(T data)
    {
        _lock.EnterWriteLock();
        try
        {
            _data.Add(data);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    public IReadOnlyList<string> Columns
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _columns.AsReadOnly();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public IReadOnlyList<Row> Rows
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _originalRows.AsReadOnly();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public Value this[int row, int column]
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                if (row < 0 || row >= _originalRows.Count)
                    return default;

                if (column < 0 || column >= _originalRows[row]._databaseValues.Count)
                    return default;

                return _originalRows[row][column];
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public Value this[int row, string column]
    {
        get => this[row, _columns.IndexOf(column)];
    }
}
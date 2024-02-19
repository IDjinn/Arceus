namespace Arceus.Core.Database.Data;

public class Table<T>
{

    internal readonly List<string> _columns = new();
    internal readonly List<Row> _originalRows = new();
    internal readonly List<T> _data = new();

    public IReadOnlyList<string> Columns => _columns.AsReadOnly();
    public IReadOnlyList<Row> Data => _originalRows.AsReadOnly();

    public Value this[int row, int column]
    {
        get
        {
            if (row < 0 || row >= _originalRows.Count)
                return default;

            if (column < 0 || column >= _originalRows[row]._databaseValues.Count)
                return default;

            return _originalRows[row][column];
        }
    }

    public Value this[int row, string column] => this[row, _columns.IndexOf(column)];
}
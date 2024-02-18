namespace Arceus.Database.Data;

public readonly record struct Value()
{
    public bool HasValue { get; } = false;
    public object? Object { get; } = null;

    public Value(object? value) : this()
    {
        HasValue = true;
        Object = value;
    }
}
namespace Arceus.Database;

public readonly record struct Value
{
    public bool HasValue { get; init; } = false;
    public object? Object { get; init; } = null;

    public Value(object? value)
    {
        HasValue = true;
        Object = value;
    }

    public Value()
    {
            
    }
}
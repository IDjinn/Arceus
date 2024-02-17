namespace Arceus.Utils;

public readonly record struct Query(string Value)
{
    public static implicit operator Query(string value) => new(value);
    public static implicit operator string(Query value) => value.Value;
}
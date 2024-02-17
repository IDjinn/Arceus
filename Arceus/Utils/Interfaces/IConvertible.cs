namespace Arceus.Utils.Interfaces;

public interface IConvertible<TSource, TValue>
{
    public TValue Parse(TSource source);
    public TSource Convert(TValue value);
}
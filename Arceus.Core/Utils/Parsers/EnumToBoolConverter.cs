namespace Arceus.Core.Utils.Parsers;

public class EnumToBoolConverter : Interfaces.IConvertible<string, bool>
{
    public bool Parse(string source)
    {
        return source.Equals("1");
    }

    public string Convert(bool value)
    {
        return value ? "1" : "0";
    }
}
using Arceus.Core.Utils.Interfaces;

namespace Arceus.Core.Tests.Console.Entities;

public class ItemTypeConverter : IConvertible<string, ItemType>
{
    public ItemType Parse(string source)
    {
        return source[0] switch
        {
            's' => ItemType.Floor,
            'i' => ItemType.Wall,
            'e' => ItemType.Effect,
            'b' => ItemType.Badge,
            'r' => ItemType.Robot,
            'h' => ItemType.HabboClub,
            'p' => ItemType.Pet,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };
    }

    public string Convert(ItemType value)
    {
        return value switch
        {
            ItemType.Floor => "s",
            ItemType.Wall => "i",
            ItemType.Effect => "e",
            ItemType.Badge => "b",
            ItemType.Robot => "r",
            ItemType.HabboClub => "h",
            ItemType.Pet => "p",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }
}
namespace Arceus.Core.Tests.Console.Entities;

[Flags]
public enum ItemType
{
    None = 0,
    Floor = 1 << 1,
    Wall = 1 << 2,
    Effect,
    Badge,
    Robot,
    HabboClub,
    Pet,
    
}
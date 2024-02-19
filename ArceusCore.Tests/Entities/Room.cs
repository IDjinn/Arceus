using ArceusCore.Database.Attributtes;
using ArceusCore.Utils.Interfaces;

namespace ArceusCore.Tests.Entities;

[Table("rooms")]
public record Room
{
    [Column("id")]
    public int Id { get; set; }
    
    [Column("name")]
    public string Name { get; set; }
    
    [Column("state")]
    [Converter(typeof(RoomStateConverter))]
    public RoomState State { get; set; }
}

public enum RoomState
{
    Open,
    Locked,
    Password,
    Invisible,
}

public class RoomStateConverter : IConvertible<string, RoomState>
{
    public RoomState Parse(string source)
    {
        return source switch
        {
            "open" => RoomState.Open,
            "locked" => RoomState.Locked,
            "password" => RoomState.Password,
            "invisible" => RoomState.Invisible,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };
    }

    public string Convert(RoomState value)
    {
        return value switch
        {
            RoomState.Open => "open",
            RoomState.Locked => "locked",
            RoomState.Password => "password",
            RoomState.Invisible => "invisible",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }
}
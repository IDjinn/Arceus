# Arceus

Simple, micro-orm to query MYSQL databases written in C#


#### No use this library for real projects, it's still in development

### Example usage

```csharp
[Table("users")]
public class User {
    [Column("id")]
    public int Id { get; init; }
    
    [Column("name")]
    public string Name { get; init; }
} 


public void Search() {
    var connection = new MysqlConnection(connectionString);
    var result = connection.Query<User>("SELECT * FROM `users`");
    _ = result.Data; // IEnumerable<User>
}
```


### Conversion Explained

Values conversion works like [c# built-in conversions](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/numeric-conversions) by default.
If you want to convert some type, must implement `IConvertible<TSource, TValue>` interface in converter class.
Example:

````csharp
public class ItemTypeConverter : IConvertible<string, ItemType>
{
    public ItemType Parse(string source)
    {
        return source[0] switch
        {
            's' => ItemType.Floor,
            'i' => ItemType.Wall,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };
    }

    public string Convert(ItemType value)
    {
        return value switch
        {
            ItemType.Floor => "s",
            ItemType.Wall => "i",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }
}
````
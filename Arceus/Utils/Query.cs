using System.Diagnostics.CodeAnalysis;
using Arceus.Database;

namespace Arceus.Utils;


public readonly record struct Query([ConstantExpected] string Value)
{
    public static implicit operator Query([ConstantExpected] string value) => new(value);
    public static implicit operator string(Query value) => value.Value;
}
using System.Diagnostics.CodeAnalysis;

namespace Arceus.Core.Utils;


public readonly record struct Query([ConstantExpected] string Value)
{
    public static implicit operator Query([ConstantExpected] string value) => new(value);
    public static implicit operator string(Query value) => value.Value;
}
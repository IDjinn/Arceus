using System.Diagnostics.CodeAnalysis;

namespace ArceusCore.Utils;


public readonly record struct Query([ConstantExpected] string QueryString, object[] Parameters)
{
    public static implicit operator Query([ConstantExpected] string value) => new(value, Array.Empty<object>());
    public static implicit operator string(Query value) => value.QueryString;
    public static implicit operator object?[](Query value) => value.Parameters;
}

public readonly record struct Record<T>(object[] AdditionalParameters);
namespace CxManager.Domain.Export;

public static class Rfc4180Escaper
{
    private static readonly char[] SpecialChars = ['"', ',', '\r', '\n'];

    public static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.IndexOfAny(anyOf: SpecialChars) < 0)
            return value;

        string escaped = value.Replace(oldValue: "\"", newValue: "\"\"", comparisonType: StringComparison.Ordinal);
        return $"\"{escaped}\"";
    }
}

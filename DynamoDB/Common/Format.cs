namespace Common;

public static class Format
{
    public static string ShowItems<TKey, TValue>(this Dictionary<TKey, TValue> dictionary) where TKey : notnull
        => $"{{\n{string.Join('\n', dictionary.Select(kv => $"{kv.Key}: {kv.Value}"))}\n}}";
}
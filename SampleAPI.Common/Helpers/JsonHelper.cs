using System.Text.Json;
using System.Text.Json.Serialization;

namespace SampleAPI.Common.Helpers;

/// <summary>
/// JSONヘルパークラス
/// </summary>
public static class JsonHelper
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// オブジェクトをJSON文字列にシリアライズ
    /// </summary>
    public static string Serialize<T>(T obj, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Serialize(obj, options ?? DefaultOptions);
    }

    /// <summary>
    /// JSON文字列をオブジェクトにデシリアライズ
    /// </summary>
    public static T? Deserialize<T>(string json, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
    }

    /// <summary>
    /// オブジェクトのディープコピー
    /// </summary>
    public static T? DeepCopy<T>(T obj)
    {
        var json = Serialize(obj);
        return Deserialize<T>(json);
    }

    /// <summary>
    /// JSON文字列の検証
    /// </summary>
    public static bool IsValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            using var doc = JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// JSONファイルから読み込み
    /// </summary>
    public static async Task<T?> ReadFromFileAsync<T>(string filePath, JsonSerializerOptions? options = null)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        var json = await File.ReadAllTextAsync(filePath);
        return Deserialize<T>(json, options);
    }

    /// <summary>
    /// JSONファイルに書き込み
    /// </summary>
    public static async Task WriteToFileAsync<T>(string filePath, T obj, JsonSerializerOptions? options = null)
    {
        var json = Serialize(obj, options);
        await File.WriteAllTextAsync(filePath, json);
    }
}

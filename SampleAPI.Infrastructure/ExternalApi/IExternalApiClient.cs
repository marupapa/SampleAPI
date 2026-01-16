namespace SampleAPI.Infrastructure.ExternalApi;

/// <summary>
/// 外部APIクライアントインターフェース
/// </summary>
public interface IExternalApiClient
{
    /// <summary>
    /// GETリクエストを送信
    /// </summary>
    Task<TResponse?> GetAsync<TResponse>(string endpoint, Dictionary<string, string>? headers = null);

    /// <summary>
    /// POSTリクエストを送信
    /// </summary>
    Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data, Dictionary<string, string>? headers = null);

    /// <summary>
    /// PUTリクエストを送信
    /// </summary>
    Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data, Dictionary<string, string>? headers = null);

    /// <summary>
    /// DELETEリクエストを送信
    /// </summary>
    Task<bool> DeleteAsync(string endpoint, Dictionary<string, string>? headers = null);

    /// <summary>
    /// レスポンス文字列を取得（パース前）
    /// </summary>
    Task<string> GetRawResponseAsync(string endpoint, Dictionary<string, string>? headers = null);
}

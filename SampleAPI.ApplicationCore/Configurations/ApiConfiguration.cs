namespace SampleAPI.ApplicationCore.Configurations;

/// <summary>
/// API設定
/// </summary>
public class ApiConfiguration
{
    /// <summary>
    /// APIベースURL
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// タイムアウト（秒）
    /// </summary>
    public int Timeout { get; set; } = 30;

    /// <summary>
    /// リトライ回数
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// APIキー
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// ユーザーエージェント
    /// </summary>
    public string UserAgent { get; set; } = "SampleAPI/1.0";

    /// <summary>
    /// SSL証明書検証を有効化
    /// </summary>
    public bool ValidateSslCertificate { get; set; } = true;
}

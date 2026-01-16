namespace SampleAPI.ApplicationCore.Configurations;

/// <summary>
/// データベース設定
/// </summary>
public class DatabaseConfiguration
{
    /// <summary>
    /// 接続文字列
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// コマンドタイムアウト（秒）
    /// </summary>
    public int CommandTimeout { get; set; } = 30;

    /// <summary>
    /// リトライ回数
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// リトライ間隔（秒）
    /// </summary>
    public int RetryIntervalSeconds { get; set; } = 5;

    /// <summary>
    /// 接続プール最小サイズ
    /// </summary>
    public int MinPoolSize { get; set; } = 5;

    /// <summary>
    /// 接続プール最大サイズ
    /// </summary>
    public int MaxPoolSize { get; set; } = 100;
}

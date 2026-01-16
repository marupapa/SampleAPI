namespace SampleAPI.Common.Logging;

/// <summary>
/// ロガーサービスインターフェース
/// </summary>
public interface ILoggerService
{
    /// <summary>
    /// デバッグレベルログ
    /// </summary>
    void Debug(string message);

    /// <summary>
    /// 情報レベルログ
    /// </summary>
    void Info(string message);

    /// <summary>
    /// 警告レベルログ
    /// </summary>
    void Warning(string message);

    /// <summary>
    /// エラーレベルログ
    /// </summary>
    void Error(string message);

    /// <summary>
    /// エラーレベルログ（例外付き）
    /// </summary>
    void Error(Exception exception, string message);

    /// <summary>
    /// 致命的エラーログ
    /// </summary>
    void Fatal(string message);

    /// <summary>
    /// 致命的エラーログ（例外付き）
    /// </summary>
    void Fatal(Exception exception, string message);

    /// <summary>
    /// 構造化ログ
    /// </summary>
    void Log<T>(LogLevel level, string message, T data);
}

/// <summary>
/// ログレベル
/// </summary>
public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Fatal
}

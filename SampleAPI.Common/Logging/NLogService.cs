using NLog;

namespace SampleAPI.Common.Logging;

/// <summary>
/// NLogベースのロガーサービス実装
/// </summary>
public class NLogService : ILoggerService
{
    private readonly ILogger _logger;

    public NLogService()
    {
        _logger = LogManager.GetCurrentClassLogger();
    }

    public NLogService(Type type)
    {
        _logger = LogManager.GetLogger(type.FullName);
    }

    public NLogService(string loggerName)
    {
        _logger = LogManager.GetLogger(loggerName);
    }

    public void Debug(string message)
    {
        _logger.Debug(message);
    }

    public void Info(string message)
    {
        _logger.Info(message);
    }

    public void Warning(string message)
    {
        _logger.Warn(message);
    }

    public void Error(string message)
    {
        _logger.Error(message);
    }

    public void Error(Exception exception, string message)
    {
        _logger.Error(exception, message);
    }

    public void Fatal(string message)
    {
        _logger.Fatal(message);
    }

    public void Fatal(Exception exception, string message)
    {
        _logger.Fatal(exception, message);
    }

    public void Log<T>(LogLevel level, string message, T data)
    {
        var logEvent = new LogEventInfo
        {
            Level = ConvertLogLevel(level),
            Message = message
        };
        
        logEvent.Properties["Data"] = data;
        _logger.Log(logEvent);
    }

    private static NLog.LogLevel ConvertLogLevel(LogLevel level)
    {
        return level switch
        {
            LogLevel.Debug => NLog.LogLevel.Debug,
            LogLevel.Info => NLog.LogLevel.Info,
            LogLevel.Warning => NLog.LogLevel.Warn,
            LogLevel.Error => NLog.LogLevel.Error,
            LogLevel.Fatal => NLog.LogLevel.Fatal,
            _ => NLog.LogLevel.Info
        };
    }
}

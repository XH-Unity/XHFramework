using System;
using System.Text;

namespace XHFramework.Core {

/// <summary>
/// 控制台日志处理器
/// 负责将日志输出到标准控制台
/// </summary>
public class ConsoleLogHandler : ILogHandler
{
    private readonly StringBuilder _formatBuffer = new StringBuilder(512);

    public void Handle(LogLevel level, string tag, string message, UnityEngine.Object context = null)
    {
        _formatBuffer.Clear();

        if (LogConfig.ShowTimestamp)
        {
            _formatBuffer.Append("[");
            _formatBuffer.Append(DateTime.Now.ToString(LogConfig.TimestampFormat));
            _formatBuffer.Append("] ");
        }

        // 添加带颜色的级别标签
        string levelTag = GetLevelTag(level);
        string levelColor = GetLevelColor(level);

        _formatBuffer.Append(levelColor);
        _formatBuffer.Append("[");
        _formatBuffer.Append(levelTag);
        _formatBuffer.Append("]");
        _formatBuffer.Append(GetResetColor());

        if (!string.IsNullOrEmpty(tag))
        {
            _formatBuffer.Append(" ");
            _formatBuffer.Append(GetTagColor());
            _formatBuffer.Append("[");
            _formatBuffer.Append(tag);
            _formatBuffer.Append("]");
            _formatBuffer.Append(GetResetColor());
        }

        _formatBuffer.Append(" ");
        _formatBuffer.Append(message);

        Console.WriteLine(_formatBuffer.ToString());
    }

    public void HandleStackTrace(string stackTrace)
    {
        if (LogConfig.ShowStackTrace && !string.IsNullOrEmpty(stackTrace))
        {
            Console.WriteLine(stackTrace);
        }
    }

    public void Flush()
    {
        // 控制台不需要刷新
    }

    public void Dispose()
    {
        _formatBuffer.Clear();
    }

    private string GetLevelTag(LogLevel level)
    {
        return level switch
        {
            LogLevel.Info => "INFO",
            LogLevel.Warn => "WARN",
            LogLevel.Error => "ERROR",
            _ => "UNKNOWN"
        };
    }

    /// <summary>
    /// 获取日志级别的 ANSI 颜色代码
    /// </summary>
    private string GetLevelColor(LogLevel level)
    {
        return level switch
        {
            LogLevel.Info => "\u001b[32m",    // 绿色
            LogLevel.Warn => "\u001b[33m",    // 黄色
            LogLevel.Error => "\u001b[31m",   // 红色
            _ => "\u001b[37m"                 // 白色
        };
    }

    /// <summary>
    /// 获取标签的 ANSI 颜色代码
    /// </summary>
    private string GetTagColor()
    {
        return "\u001b[36m"; // 青色
    }

    /// <summary>
    /// 获取重置颜色的 ANSI 代码
    /// </summary>
    private string GetResetColor()
    {
        return "\u001b[0m"; // 重置
    }
}

}

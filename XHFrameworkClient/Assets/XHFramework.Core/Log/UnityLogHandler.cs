using System;
using System.Text;
using UnityEngine;

namespace XHFramework.Core {

/// <summary>
/// Unity日志处理器
/// 负责将日志输出到Unity Debug和控制台
/// </summary>
public class UnityLogHandler : ILogHandler
{
    private readonly StringBuilder _formatBuffer = new StringBuilder(512);
    private string _lastTag = "";

    public void Handle(LogLevel level, string tag, string message, UnityEngine.Object context = null)
    {
        _formatBuffer.Clear();

        // 构建格式化消息
        if (LogConfig.ShowTimestamp)
        {
            _formatBuffer.Append(DateTime.Now.ToString(LogConfig.TimestampFormat));
            _formatBuffer.Append(" ");
        }

        string levelTag = GetLevelTag(level);
        string colorCode = GetLevelColor(level);

        _formatBuffer.Append("<color=");
        _formatBuffer.Append(colorCode);
        _formatBuffer.Append(">[");
        _formatBuffer.Append(levelTag);
        _formatBuffer.Append("]</color>");

        if (!string.IsNullOrEmpty(tag))
        {
            _formatBuffer.Append(" <color=#00BFFF>");
            _formatBuffer.Append(tag);
            _formatBuffer.Append("</color>");
        }

        _formatBuffer.Append(" ");
        _formatBuffer.Append(message);

        string formattedMessage = _formatBuffer.ToString();

        // 输出到Unity Debug
        switch (level)
        {
            case LogLevel.Info:
                UnityEngine.Debug.Log(formattedMessage, context);
                break;
            case LogLevel.Warn:
                UnityEngine.Debug.LogWarning(formattedMessage, context);
                break;
            case LogLevel.Error:
                UnityEngine.Debug.LogError(formattedMessage, context);
                break;
        }

        _lastTag = tag;
    }

    public void HandleStackTrace(string stackTrace)
    {
        if (LogConfig.ShowStackTrace && !string.IsNullOrEmpty(stackTrace))
        {
            UnityEngine.Debug.Log(stackTrace);
        }
    }

    public void Flush()
    {
        // Unity不需要刷新
    }

    public void Dispose()
    {
        _formatBuffer.Clear();
    }

    private string GetLevelTag(LogLevel level)
    {
        return level switch
        {
            LogLevel.Info => "Info",
            LogLevel.Warn => "Warn",
            LogLevel.Error => "Error",
            _ => "Unknown"
        };
    }

    private string GetLevelColor(LogLevel level)
    {
        return level switch
        {
            LogLevel.Info => "#008000",    // 绿色
            LogLevel.Warn => "#FFFF00",    // 黄色
            LogLevel.Error => "#FF0000",   // 红色
            _ => "#FFFFFF"                 // 白色
        };
    }
}

}

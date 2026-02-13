using System;
using UnityEngine;

namespace XHFramework.Core {

/// <summary>
/// 简化的日志管理器 - 自动标签版本
/// 无需手动指定标签，自动从调用栈推导
/// </summary>
public static class Log
{
    /// <summary>
    /// 输出信息日志（自动推导来源）
    /// </summary>
    [System.Diagnostics.Conditional("EnableLog")]
    public static void Info(string message)
    {
        if (LogConfig.CurrentLevel <= LogLevel.Info)
            InternalLog(LogLevel.Info, message);
    }

    /// <summary>
    /// 输出警告日志（自动推导来源）
    /// </summary>
    [System.Diagnostics.Conditional("EnableLog")]
    public static void Warn(string message)
    {
        if (LogConfig.CurrentLevel <= LogLevel.Warn)
            InternalLog(LogLevel.Warn, message);
    }

    /// <summary>
    /// 输出错误日志（自动推导来源）
    /// </summary>
    [System.Diagnostics.Conditional("EnableLog")]
    public static void Error(string message)
    {
        if (LogConfig.CurrentLevel <= LogLevel.Error)
            InternalLog(LogLevel.Error, message);
    }

    /// <summary>
    /// 输出格式化的信息日志
    /// </summary>
    [System.Diagnostics.Conditional("EnableLog")]
    public static void Info(string format, params object[] args)
    {
        if (LogConfig.CurrentLevel <= LogLevel.Info)
            InternalLog(LogLevel.Info, string.Format(format, args));
    }

    /// <summary>
    /// 输出格式化的警告日志
    /// </summary>
    [System.Diagnostics.Conditional("EnableLog")]
    public static void Warn(string format, params object[] args)
    {
        if (LogConfig.CurrentLevel <= LogLevel.Warn)
            InternalLog(LogLevel.Warn, string.Format(format, args));
    }

    /// <summary>
    /// 输出格式化的错误日志
    /// </summary>
    [System.Diagnostics.Conditional("EnableLog")]
    public static void Error(string format, params object[] args)
    {
        if (LogConfig.CurrentLevel <= LogLevel.Error)
            InternalLog(LogLevel.Error, string.Format(format, args));
    }

    /// <summary>
    /// 输出异常日志（自动包含堆栈）
    /// </summary>
    [System.Diagnostics.Conditional("EnableLog")]
    public static void Exception(Exception ex, string message = "")
    {
        if (LogConfig.CurrentLevel <= LogLevel.Error)
        {
            string fullMessage = string.IsNullOrEmpty(message)
                ? ex?.Message ?? "Unknown exception"
                : $"{message}: {ex?.Message}";
            InternalLog(LogLevel.Error, fullMessage);
        }
    }

    /// <summary>
    /// 内部日志处理 - 自动推导调用者信息
    /// </summary>
    private static void InternalLog(LogLevel level, string message)
    {
        // 自动推导来源（从调用栈）
        var (fileName, lineNumber, methodName) = StackTraceProvider.GetCallerInfo();

        // 生成简洁的标签（方法名或类名.方法名）
        string tag = methodName;

        // 根据日志级别调用对应的 Logger 方法
        switch (level)
        {
            case LogLevel.Info:
                Logger.Info(tag, message);
                break;
            case LogLevel.Warn:
                Logger.Warn(tag, message);
                break;
            case LogLevel.Error:
                Logger.Error(tag, message);
                break;
        }
    }
}

}

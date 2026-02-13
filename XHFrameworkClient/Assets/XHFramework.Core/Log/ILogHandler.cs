using System;

namespace XHFramework.Core {

/// <summary>
/// 日志处理器接口
/// 所有日志处理器都需要实现此接口
/// </summary>
public interface ILogHandler
{
    /// <summary>
    /// 处理日志消息
    /// </summary>
    /// <param name="level">日志级别</param>
    /// <param name="tag">标签</param>
    /// <param name="message">消息内容</param>
    /// <param name="context">上下文对象（可选）</param>
    void Handle(LogLevel level, string tag, string message, UnityEngine.Object context = null);

    /// <summary>
    /// 处理堆栈信息
    /// </summary>
    /// <param name="stackTrace">堆栈信息</param>
    void HandleStackTrace(string stackTrace);

    /// <summary>
    /// 刷新缓冲区（用于文件写入）
    /// </summary>
    void Flush();

    /// <summary>
    /// 释放资源
    /// </summary>
    void Dispose();
}

}

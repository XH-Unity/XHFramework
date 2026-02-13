using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Reflection;

namespace XHFramework.Core {

/// <summary>
/// 增强的堆栈信息提供器
/// 提供详细的调用堆栈信息，包含源代码链接
/// </summary>
public static class StackTraceProvider
{
    /// <summary>
    /// 获取增强的堆栈信息，包含文件名和行号
    /// </summary>
    /// <param name="depth">堆栈深度，0表示完整</param>
    /// <returns>格式化的堆栈信息</returns>
    public static string GetEnhancedStackTrace(int depth = 0)
    {
        var stackTrace = new StackTrace(true);
        var frames = stackTrace.GetFrames();

        if (frames == null || frames.Length == 0)
            return "No stack trace available";

        var sb = new StringBuilder();
        sb.AppendLine("========== Stack Trace ==========");

        int maxDepth = (depth > 0 && depth < frames.Length) ? depth : frames.Length;

        // 从1开始跳过当前方法
        for (int i = 1; i < maxDepth; i++)
        {
            var frame = frames[i];
            var method = frame.GetMethod();

            if (method == null)
                continue;

            // 跳过日志相关的内部调用
            if (IsLoggerInternalMethod(method))
                continue;

            sb.Append("  at ");

            // 添加类名和方法名
            if (method.DeclaringType != null)
            {
                sb.Append(method.DeclaringType.FullName);
                sb.Append(".");
            }
            sb.Append(method.Name);

            // 添加参数信息
            sb.Append("(");
            var parameters = method.GetParameters();
            for (int j = 0; j < parameters.Length; j++)
            {
                if (j > 0) sb.Append(", ");
                sb.Append(parameters[j].ParameterType.Name);
            }
            sb.Append(")");

            // 添加源代码位置信息
            string fileName = frame.GetFileName();
            int lineNumber = frame.GetFileLineNumber();
            int columnNumber = frame.GetFileColumnNumber();

            if (!string.IsNullOrEmpty(fileName))
            {
                sb.Append(" in ");
                sb.Append(fileName);

                if (lineNumber > 0)
                {
                    sb.Append(":");
                    sb.Append(lineNumber);

                    if (columnNumber > 0)
                    {
                        sb.Append(":");
                        sb.Append(columnNumber);
                    }
                }
            }
            else if (frame.GetILOffset() >= 0)
            {
                sb.Append(" [IL 0x").Append(frame.GetILOffset().ToString("X4")).Append("]");
            }

            sb.AppendLine();
        }

        sb.AppendLine("=================================");
        return sb.ToString();
    }

    /// <summary>
    /// 获取调用者信息（立即调用者）
    /// </summary>
    /// <returns>调用者的文件名、行号和方法名</returns>
    public static (string FileName, int LineNumber, string MethodName) GetCallerInfo()
    {
        var stackTrace = new StackTrace(true);
        var frames = stackTrace.GetFrames();

        if (frames == null || frames.Length < 2)
            return ("Unknown", 0, "Unknown");

        // 跳过当前方法和Logger内部方法，找到真正的调用者
        for (int i = 1; i < frames.Length; i++)
        {
            var frame = frames[i];
            var method = frame.GetMethod();

            if (method != null && !IsLoggerInternalMethod(method))
            {
                string fileName = Path.GetFileName(frame.GetFileName() ?? "Unknown");
                int lineNumber = frame.GetFileLineNumber();
                string methodName = method.Name;

                return (fileName, lineNumber, methodName);
            }
        }

        return ("Unknown", 0, "Unknown");
    }

    /// <summary>
    /// 检查是否为Logger内部方法
    /// </summary>
    private static bool IsLoggerInternalMethod(MethodBase method)
    {
        if (method.DeclaringType == null)
            return false;

        string typeName = method.DeclaringType.Name;
        return typeName == "Logger" ||
               typeName == "LoggerExtensions" ||
               typeName == "StackTraceProvider" ||
               typeName == "Debuger" ||           // 兼容旧名称
               typeName == "LogUtlis";            // 兼容旧名称
    }

    /// <summary>
    /// 获取简洁的堆栈信息（仅显示关键方法）
    /// </summary>
    public static string GetSimpleStackTrace()
    {
        var (fileName, lineNumber, methodName) = GetCallerInfo();
        return $"at {methodName} ({fileName}:{lineNumber})";
    }
}

}

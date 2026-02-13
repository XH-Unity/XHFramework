using System;
using System.IO;
using System.Text;
using System.Threading;

namespace XHFramework.Core {

/// <summary>
/// 文件日志处理器 - 线程安全版本
/// 负责将日志写入文件，支持日志轮转
/// </summary>
public class FileLogHandler : ILogHandler, IDisposable
{
    private StreamWriter _fileWriter;
    private readonly StringBuilder _formatBuffer = new StringBuilder(512);
    private readonly object _lockObject = new object();
    private string _currentLogFilePath;
    private long _currentFileSize;

    public FileLogHandler()
    {
        LogConfig.Validate();
        InitializeLogFile();
    }

    public void Handle(LogLevel level, string tag, string message, UnityEngine.Object context = null)
    {
        if (!LogConfig.SaveToFile || _fileWriter == null)
            return;

        lock (_lockObject)
        {
            try
            {
                _formatBuffer.Clear();

                if (LogConfig.ShowTimestamp)
                {
                    _formatBuffer.Append("[");
                    _formatBuffer.Append(DateTime.Now.ToString(LogConfig.TimestampFormat));
                    _formatBuffer.Append("] ");
                }

                _formatBuffer.Append("[");
                _formatBuffer.Append(GetLevelTag(level));
                _formatBuffer.Append("]");

                if (!string.IsNullOrEmpty(tag))
                {
                    _formatBuffer.Append(" [");
                    _formatBuffer.Append(tag);
                    _formatBuffer.Append("]");
                }

                _formatBuffer.Append(" ");
                _formatBuffer.Append(message);

                string logLine = _formatBuffer.ToString();
                byte[] logBytes = Encoding.UTF8.GetBytes(logLine + Environment.NewLine);

                // 检查是否需要轮转文件
                if (LogConfig.MaxLogFileSize > 0 &&
                    _currentFileSize + logBytes.Length > LogConfig.MaxLogFileSize)
                {
                    RotateLogFile();
                }

                _fileWriter.WriteLine(logLine);
                _currentFileSize += logBytes.Length;

                // 每写入一条日志就刷新，确保数据及时写入磁盘
                _fileWriter.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FileLogHandler Error] {ex.Message}");
            }
        }
    }

    public void HandleStackTrace(string stackTrace)
    {
        if (!LogConfig.ShowStackTrace || string.IsNullOrEmpty(stackTrace) || _fileWriter == null)
            return;

        lock (_lockObject)
        {
            try
            {
                _fileWriter.WriteLine(stackTrace);
                _fileWriter.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FileLogHandler Stack Error] {ex.Message}");
            }
        }
    }

    public void Flush()
    {
        lock (_lockObject)
        {
            try
            {
                _fileWriter?.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FileLogHandler Flush Error] {ex.Message}");
            }
        }
    }

    public void Dispose()
    {
        lock (_lockObject)
        {
            try
            {
                _fileWriter?.Flush();
                _fileWriter?.Dispose();
                _fileWriter = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FileLogHandler Dispose Error] {ex.Message}");
            }
        }
    }

    private void InitializeLogFile()
    {
        lock (_lockObject)
        {
            try
            {
                // 确定日志目录
                if (string.IsNullOrEmpty(LogConfig.LogFileDirectory))
                {
                    LogConfig.LogFileDirectory = GetDefaultLogDirectory();
                }

                // 创建目录
                if (!Directory.Exists(LogConfig.LogFileDirectory))
                {
                    Directory.CreateDirectory(LogConfig.LogFileDirectory);
                }

                // 生成日志文件名
                string timestamp = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
                string logFileName = $"Log_{timestamp}.log";
                _currentLogFilePath = Path.Combine(LogConfig.LogFileDirectory, logFileName);

                // 创建或追加到文件
                _fileWriter = new StreamWriter(_currentLogFilePath, true, Encoding.UTF8, 8192)
                {
                    AutoFlush = false
                };

                _currentFileSize = new FileInfo(_currentLogFilePath).Length;

                // 清理旧日志文件
                CleanOldLogFiles();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FileLogHandler Init Error] {ex.Message}");
                _fileWriter = null;
            }
        }
    }

    private void RotateLogFile()
    {
        try
        {
            _fileWriter?.Flush();
            _fileWriter?.Dispose();

            string timestamp = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
            string logFileName = $"Log_{timestamp}.log";
            _currentLogFilePath = Path.Combine(LogConfig.LogFileDirectory, logFileName);

            _fileWriter = new StreamWriter(_currentLogFilePath, true, Encoding.UTF8, 8192)
            {
                AutoFlush = false
            };

            _currentFileSize = 0;

            // 清理旧文件
            CleanOldLogFiles();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FileLogHandler Rotate Error] {ex.Message}");
        }
    }

    private void CleanOldLogFiles()
    {
        try
        {
            var dirInfo = new DirectoryInfo(LogConfig.LogFileDirectory);
            var logFiles = dirInfo.GetFiles("Log_*.log");

            if (logFiles.Length > LogConfig.MaxLogFileCount)
            {
                // 按修改时间排序，删除最旧的
                Array.Sort(logFiles, (a, b) => a.LastWriteTime.CompareTo(b.LastWriteTime));

                int filesToDelete = logFiles.Length - LogConfig.MaxLogFileCount;
                for (int i = 0; i < filesToDelete; i++)
                {
                    try
                    {
                        logFiles[i].Delete();
                    }
                    catch { }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FileLogHandler Clean Error] {ex.Message}");
        }
    }

    private string GetDefaultLogDirectory()
    {
#if UNITY_EDITOR
        string projectRoot = Directory.GetParent(UnityEngine.Application.dataPath).FullName;
        return Path.Combine(projectRoot, "../XHFrameworkOut/Logs");
#elif UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
        return UnityEngine.Application.persistentDataPath + "/Logs/";
#else
        return AppDomain.CurrentDomain.BaseDirectory + "Logs/";
#endif
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
}

}

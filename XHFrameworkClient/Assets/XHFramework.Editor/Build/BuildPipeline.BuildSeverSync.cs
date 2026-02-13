using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using XHFramework.Core;
namespace XHFramework.Editor {

public partial class BuildPipelineEditor
{
    // 服务器资源根目录配置
    private static string ServerResRoot =Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).FullName, "../XHFrameworkServer/nginx-1.28.0/html/XHFramework/Res");

    // 完整的服务器资源目录路径
    private static string ServerResDir => GetPlatformURL(ServerResRoot);
    // Bundles 源目录路径
    private static string BundlesDir => Path.Combine(BuildToolPanel.ProjectRoot, "Bundles", GetPlatformName(), "DefaultPackage",PlayerSettings.bundleVersion);
    
    /// <summary>
    /// 增量同步：只同步资源
    /// </summary>
    static void BuildSeverSync()
    {
        SeverSyncRes();
    }

    /// <summary>
    /// 同步资源到服务器：删除目标文件夹内容，然后复制源文件夹内容
    /// </summary>
    static void SeverSyncRes()
    {
        try
        {
            Log.Info($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 开始资源同步");
            Log.Info($"源目录: {BundlesDir}");
            Log.Info($"目标目录: {ServerResDir}");

            // 检查源目录是否存在
            if (!Directory.Exists(BundlesDir))
            {
                Log.Info($"❌ 错误: Bundles目录不存在: {BundlesDir}");
                throw new DirectoryNotFoundException($"Bundles目录不存在: {BundlesDir}");
            }

            // 第一步：删除服务器目标文件夹内所有内容
            if (Directory.Exists(ServerResDir))
            {
                Log.Info("正在清空服务器目录...");
                DirectoryInfo di = new DirectoryInfo(ServerResDir);
                foreach (FileInfo file in di.GetFiles("*", SearchOption.AllDirectories))
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
            }
            else
            {
                Directory.CreateDirectory(ServerResDir);
            }

            // 第二步：将BundlesDir文件夹内容复制到服务器文件夹
            Log.Info("正在复制资源文件...");
            CopyDirectory(BundlesDir, ServerResDir, true);

            Log.Info("✅ 资源同步完成!");
        }
        catch (Exception ex)
        {
            Log.Info($"❌ 资源同步失败: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 递归复制目录
    /// </summary>
    private static void CopyDirectory(string sourceDir, string targetDir, bool overwrite)
    {
        DirectoryInfo dir = new DirectoryInfo(sourceDir);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException($"源目录不存在: {sourceDir}");
        }

        // 创建目标目录
        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        // 复制所有文件（使用文件过滤规则）
        foreach (FileInfo file in dir.GetFiles())
        {
            if (ShouldUploadFile(file.Name))
            {
                string targetFilePath = Path.Combine(targetDir, file.Name);
                file.CopyTo(targetFilePath, overwrite);
                Log.Info($"已上传: {file.Name}");
            }
            else
            {
                Log.Info($"跳过文件: {file.Name}");
            }
        }

        // 递归复制子目录
        foreach (DirectoryInfo subDir in dir.GetDirectories())
        {
            string newTargetDir = Path.Combine(targetDir, subDir.Name);
            CopyDirectory(subDir.FullName, newTargetDir, overwrite);
        }
    }

    /// <summary>
    /// 判断文件是否应该上传
    /// </summary>
    private static bool ShouldUploadFile(string fileName)
    {
        // 需要上传的文件规则
        var uploadPatterns = new[]
        {
            "*.version",          // 本地版本文件
            "*.bundle",           // AssetBundle 文件
            "*_*.bytes",          // 清单文件
            "*_*.hash",           // 哈希文件
            "*_*.json"            // JSON 清单文件
        };

        // 不需要上传的文件规则
        var ignorePatterns = new[]
        {
            "*.report",           // 构建报告
            "buildlog*.json",     // 构建日志
            "link.xml",           // 构建配置
        };

        // 先检查是否在忽略列表中
        if (ignorePatterns.Any(pattern => MatchPattern(fileName, pattern)))
        {
            return false;
        }

        // 再检查是否在上传列表中
        if (uploadPatterns.Any(pattern => MatchPattern(fileName, pattern)))
        {
            return true;
        }

        // 默认不上传未匹配的文件
        return false;
    }

    /// <summary>
    /// 简单的通配符匹配
    /// </summary>
    private static bool MatchPattern(string fileName, string pattern)
    {
        // 将通配符模式转换为正则表达式
        string regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
            .Replace("\\*", ".*")
            .Replace("\\?", ".") + "$";

        return System.Text.RegularExpressions.Regex.IsMatch(
            fileName,
            regexPattern,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
    }
    
    /// <summary>
    /// 根据当前平台获取完整的资源URL
    /// </summary>
    private static string GetPlatformURL(string baseURL)
    {
        string platformName = GetPlatformName();
        // 确保baseURL末尾没有斜杠
        baseURL = baseURL.TrimEnd('/');
        return $"{baseURL}/{platformName}";
    }

    /// <summary>
    /// 获取当前平台名称
    /// </summary>
    private static string GetPlatformName()
    {
#if UNITY_ANDROID
        return "Android";
#elif UNITY_IOS
        return "iOS";
#elif UNITY_WEBGL
        return "WebGL";
#elif UNITY_STANDALONE_WIN
        return "Windows";
#elif UNITY_STANDALONE_OSX
        return "MacOS";
#elif UNITY_STANDALONE_LINUX
        return "Linux";
#else
        return "Default";
#endif
    }
}

}
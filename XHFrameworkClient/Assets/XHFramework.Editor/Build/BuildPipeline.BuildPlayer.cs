using System;
using System.Linq;
using UnityEditor;
using BuildReport = UnityEditor.Build.Reporting.BuildReport;
using UnityEngine;
using System.IO;
using XHFramework.Core;

namespace XHFramework.Editor
{
    /// <summary>
    /// 构建目标平台枚举
    /// </summary>
    public enum BuildPlatform
    {
        Android,
        iOS
    }

    public partial class BuildPipelineEditor
    {
        /// <summary>
        /// 构建APK包（Android）
        /// </summary>
        /// <param name="buildName">构建名称前缀</param>
        public static void BuildPlayer(string buildName)
        {
            BuildPlayer(buildName, BuildPlatform.Android);
        }

        /// <summary>
        /// 构建iOS Xcode工程
        /// </summary>
        /// <param name="buildName">构建名称前缀</param>
        public static void BuildiOSPlayer(string buildName)
        {
            BuildPlayer(buildName, BuildPlatform.iOS);
        }

        /// <summary>
        /// 通用构建方法
        /// </summary>
        /// <param name="buildName">构建名称前缀</param>
        /// <param name="platform">目标平台</param>
        public static void BuildPlayer(string buildName, BuildPlatform platform)
        {
            string platformName = platform == BuildPlatform.iOS ? "iOS" : "Android";
            Log.Info($"开始{platformName}构建...");
            Log.Info($"构建类型: {buildName}");

            // 从 PlayerSettings 读取版本号
            string currentVersion = PlayerSettings.bundleVersion;

            // 确保资源刷新
            AssetDatabase.Refresh();

            // 使用可配置的输出目录
            string outputDir = platform == BuildPlatform.iOS
                ? BuildToolPanel.iOSOutputDir
                : BuildToolPanel.ApkOutputDir;

            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
                Log.Info($"创建输出目录: {outputDir}");
            }

            // 生成输出路径
            string outputPath;
            BuildTarget target;

            if (platform == BuildPlatform.iOS)
            {
                // iOS输出为Xcode工程目录
                outputPath = Path.Combine(outputDir, $"{buildName}_{currentVersion}_iOS");
                target = BuildTarget.iOS;

                // 设置iOS特定配置
                ConfigureiOSBuildSettings();
            }
            else
            {
                // Android输出为APK文件
                outputPath = Path.Combine(outputDir, $"{buildName}_{currentVersion}.apk");
                target = BuildTarget.Android;
            }

            Log.Info($"输出路径: {outputPath}");

            // 配置构建选项
            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Select(s => s.path).ToArray(),
                locationPathName = outputPath,
                target = target,
                options = BuildOptions.None
            };

            // 执行构建
            BuildReport report = UnityEditor.BuildPipeline.BuildPlayer(options);

            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                if (platform == BuildPlatform.iOS)
                {
                    Log.Info($"✅ iOS Xcode工程导出成功: {outputPath}");
                    Log.Info("📱 请将此目录复制到Mac电脑，使用Xcode打开并构建IPA");
                    Log.Info("📝 Xcode操作步骤:");
                    Log.Info("   1. 打开 Unity-iPhone.xcworkspace");
                    Log.Info("   2. 选择 Signing & Capabilities，配置Team和证书");
                    Log.Info("   3. Product → Archive");
                    Log.Info("   4. Distribute App → 导出IPA");
                }
                else
                {
                    Log.Info($"✅ APK构建成功: {outputPath}");

                    // 获取实际文件大小
                    FileInfo fileInfo = new FileInfo(outputPath);
                    long fileSizeBytes = fileInfo.Length;
                    double fileSizeMB = fileSizeBytes / (1024.0 * 1024.0);
                    double fileSizeKB = fileSizeBytes / 1024.0;
                    Log.Info($"实际文件大小: {fileSizeMB:F2} MB ({fileSizeKB:F0} KB)");
                }
            }
            else
            {
                throw new Exception($"{platformName}构建失败: {report.summary.result}");
            }
        }

        /// <summary>
        /// 配置iOS构建设置
        /// </summary>
        private static void ConfigureiOSBuildSettings()
        {
            // 设置iOS基本配置
            PlayerSettings.iOS.appleEnableAutomaticSigning = false; // 禁用自动签名，让用户在Xcode中配置
            PlayerSettings.iOS.targetOSVersionString = "12.0"; // 最低iOS版本

            // 架构设置 - 只支持ARM64
            PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, 1); // 1 = ARM64

            // 设置脚本后端为IL2CPP（iOS必须）
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);

            Log.Info("iOS构建配置:");
            Log.Info($"  - Bundle Identifier: {PlayerSettings.applicationIdentifier}");
            Log.Info($"  - 最低iOS版本: {PlayerSettings.iOS.targetOSVersionString}");
            Log.Info($"  - 架构: ARM64");
            Log.Info($"  - 脚本后端: IL2CPP");
        }

        /// <summary>
        /// 获取iOS构建版本号
        /// </summary>
        public static string GetiOSBuildNumber(string version)
        {
            // iOS的Build号通常是整数，这里取主版本号
            var match = System.Text.RegularExpressions.Regex.Match(version, @"^(\d+)\.(\d+)\.(\d+)$");
            if (!match.Success)
                return "1";

            int major = int.Parse(match.Groups[1].Value);
            int minor = int.Parse(match.Groups[2].Value);
            int patch = int.Parse(match.Groups[3].Value);

            // 组合成一个整数: major * 10000 + minor * 100 + patch
            return (major * 10000 + minor * 100 + patch).ToString();
        }
    }
}

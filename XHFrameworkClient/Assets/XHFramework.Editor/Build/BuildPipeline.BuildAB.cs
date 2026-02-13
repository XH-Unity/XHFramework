using System;
using System.Linq;
using UnityEditor;
using YooAsset.Editor;
using UnityEngine;
using System.IO;
using YooAsset;
using XHFramework.Core;

namespace XHFramework.Editor {

public partial class BuildPipelineEditor
{
    /// <summary>
    /// 构建热更新资源包
    /// </summary>
    /// <param name="autoIncrementVersion">是否自动递增版本号（开发模式传false，正式发布传true）</param>
    public static void BuildAB(bool isFullBuild,bool IsClearAndCopyAll)
    {
        // 从 PlayerSettings 读取当前版本
        string currentVersion = PlayerSettings.bundleVersion;
        string newVersion = currentVersion;
        newVersion = GetNextVersion(currentVersion, isFullBuild);

        var buildParams = new ScriptableBuildParameters
        {
            BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot(), //Yooasset 默认输出路径
            BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot(), //Yooasset 默认复制Streamming路径
            BuildPipeline = nameof(ScriptableBuildPipeline), //默认构建管线
            BuildBundleType = (int)EBuildBundleType.AssetBundle, //构建格式
            BuildTarget = EditorUserBuildSettings.activeBuildTarget, //构建平台
            PackageName = "DefaultPackage", //包名
            PackageVersion = newVersion, // 次版本号升级
            EnableSharePackRule = !isFullBuild, //是否共享
            VerifyBuildingResult = true, //验证结果
            FileNameStyle = EFileNameStyle.HashName, //资源包样式
            BuildinFileCopyOption =IsClearAndCopyAll? EBuildinFileCopyOption.ClearAndCopyAll:EBuildinFileCopyOption.None, // 清理streamming并全部复制
            BuildinFileCopyParams = null, ////需要复制到stremming的标签
            CompressOption = ECompressOption.LZ4, //压缩格式
            ClearBuildCacheFiles = true, //打包输出路径是否清除以前的
            EncryptionServices = new EncryptionNone(), //资源包加密服务
            ManifestProcessServices = new ManifestProcessNone(), //资源清单加密服务
            ManifestRestoreServices = new ManifestRestoreNone(), //资源清单解密服务
            BuiltinShadersBundleName = string.Empty, //是否内置着色器资源包名称
            DisableWriteTypeTree = false, // 全量包不禁用TypeTree写入
        };
        ExecuteBuildAB(buildParams, "热更新资源包");

        //更新版本号
        PlayerSettings.bundleVersion = newVersion;
        PlayerSettings.Android.bundleVersionCode =isFullBuild?GetAndroidVersion(newVersion): PlayerSettings.Android.bundleVersionCode;
        Log.Info($"已更新版本号: {newVersion}, Android构建号: { PlayerSettings.Android.bundleVersionCode }");
    }



    /// <summary>
    /// 执行资源包构建流程
    /// </summary>
    private static void ExecuteBuildAB(ScriptableBuildParameters buildParams, string buildName)
    {
        Log.Info($"开始执行 {buildName} 构建...");
        Log.Info($"构建参数: 目标平台={buildParams.BuildTarget}, 版本={buildParams.PackageVersion}");

        var pipeline = new ScriptableBuildPipeline();
        var result = pipeline.Run(buildParams, false);

        Log.Info($"YooAsset构建完成: {(result.Success ? "成功 ✅" : "失败 ❌")}");

        if (result.Success)
        {
            Log.Info($"✅ {buildName}构建成功 | 版本: {buildParams.PackageVersion}");

            // 资源验证和日志
            AssetDatabase.Refresh();
            string targetDir = buildParams.BuildinFileRoot;

            if (Directory.Exists(targetDir))
            {
                var files = Directory.GetFiles(targetDir, "*", SearchOption.AllDirectories);
                Log.Info($"构建成功! 资源文件数量: {files.Length}");

                if (files.Length > 0)
                {
                    Log.Info("资源文件示例:");
                    foreach (var file in files.Take(3))
                    {
                        Log.Info($"- {file.Replace(Application.dataPath, "Assets")}");
                    }
                }
                // 复制 link.xml 到 AOT/Settings/YooAsset 目录
                CopyLinkXmlToYooAsset($"{GetPlatformURL(buildParams.BuildOutputRoot)}/{buildParams.PackageName}/{buildParams.PackageVersion}");
            }
        }
        else
        {
            Log.Error($"❌ {buildName}构建失败，请检查错误日志");
            throw new Exception($"{buildName} 构建失败，已中断后续流程！");
        }
    }

    /// <summary>
    /// 复制 link.xml 文件到 AOT/Settings/YooAsset 目录
    /// </summary>
    private static void CopyLinkXmlToYooAsset(string sourceDir)
    {
        string sourceLinkXml = Path.Combine(sourceDir, "link.xml");

        if (!File.Exists(sourceLinkXml))
        {
            Log.Warn($"未找到 link.xml 文件: {sourceLinkXml}");
            return;
        }

        string targetFolder = Path.Combine(Application.dataPath, "XHFramework.Third", "YooAsset");
        string targetLinkXml = Path.Combine(targetFolder, "link.xml");

        try
        {
            // 确保目标目录存在
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
                Log.Info($"创建目录: {targetFolder}");
            }

            // 复制文件（如果存在则覆盖）
            File.Copy(sourceLinkXml, targetLinkXml, true);
            Log.Info($"✅ 已复制 link.xml 到: {targetLinkXml.Replace(Application.dataPath, "Assets")}");

            // 刷新资源数据库
            AssetDatabase.Refresh();
        }
        catch (Exception ex)
        {
            Log.Error($"❌ 复制 link.xml 失败: {ex.Message}");
        }
    }
}

}
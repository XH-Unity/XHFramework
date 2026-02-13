using UnityEditor;
using HybridCLR.Editor.Commands;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using XHFramework.Core;
namespace XHFramework.Editor {

    public partial class BuildPipelineEditor
    {
        public static void BuildDLL()
        {
            // 确保目录刷新
            AssetDatabase.Refresh();

            // 5. 生成HybridCLR所需的DLL
            Log.Info("生成HybridCLR热更DLL和AOT元数据DLL...");
            PrebuildCommand.GenerateAll();
            Log.Info("开始处理DLL文件...");

            CopyDLL(true);  // 拷贝AOT DLL
            CopyDLL(false); // 拷贝JIT DLL

            // 建议在最后刷新AssetDatabase
            AssetDatabase.Refresh();
            Log.Info("DLL处理完成！");
        }

        public static void CopyDLL(bool isAOT)
        {
            string sourceDir;
            string targetDir;
            List<string> dllList;
            string dllType = isAOT ? "AOT" : "JIT"; // 用于日志
            
            if (isAOT)
            {
                sourceDir = HybridCLR.Editor.SettingsUtil.GetAssembliesPostIl2CppStripDir(
                    EditorUserBuildSettings.activeBuildTarget);
                targetDir = BuildToolPanel.AotDllDir;
                dllList = BuildToolPanel.GetAotDLLNames();
            }
            else
            {
                sourceDir = HybridCLR.Editor.SettingsUtil.GetHotUpdateDllsOutputDirByTarget(
                    EditorUserBuildSettings.activeBuildTarget);
                targetDir = BuildToolPanel.JitDllDir;
                dllList = BuildToolPanel.GetJITDLLNames();
            }

            // 检查源目录是否存在
            if (!Directory.Exists(sourceDir))
            {
                Log.Error($"{dllType} DLL源目录不存在: {sourceDir}");
                return;
            }

            // 确保目标目录存在
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
                Log.Info($"已创建{dllType} DLL目录: {targetDir}");
            }
            
            int copiedCount = 0;
            int skippedCount = 0;
            
            foreach (string dllPath in Directory.GetFiles(sourceDir, "*.dll"))
            {
                string fileName = Path.GetFileName(dllPath);
                
                // 智能过滤：只拷贝需要的 DLL
                if (dllList.Count > 0 && !dllList.Contains(fileName))
                {
                    Log.Info($"跳过不需要的{dllType} DLL: {fileName}");
                    skippedCount++;
                    continue;
                }

                try
                {
                    // 目标路径
                    string targetPath = Path.Combine(targetDir, $"{fileName}.bytes");
                    File.Copy(dllPath, targetPath, true);

                    copiedCount++;
                    Log.Info($"已拷贝{dllType} DLL: {fileName} → {targetPath}");
                }
                catch (System.Exception e)
                {
                    Log.Error($"拷贝{dllType} DLL失败: {fileName}, 错误: {e.Message}");
                }
            }

            Log.Info($"完成{dllType} DLL拷贝: 成功 {copiedCount} 个, 跳过 {skippedCount} 个");
        }
    }

}

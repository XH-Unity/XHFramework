using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Linq;
using YooAsset.Editor;
using Object = UnityEngine.Object;
using XHFramework.Core;
namespace XHFramework.Editor {

public partial class BuildPipelineEditor
    {
        /// <summary>
        /// 构建前的准备工作：
        /// 1. 清理输出目录
        /// 2. 生成HybridCLR所需的DLL
        /// 3. 拷贝AOT和热更DLL到指定位置
        /// </summary>
        public static void BuildInit(bool buildAPK)
        {
            Log.Info("开始构建环境准备...");

            // 清空AOT DLL目录内容（保留目录）
            string aotDllDir = BuildToolPanel.AotDllDir;
            ClearDirectoryContents(aotDllDir, "AOT DLL");

            // 清空JIT DLL目录内容（保留目录）
            string jitDllDir = BuildToolPanel.JitDllDir;
            ClearDirectoryContents(jitDllDir, "JIT DLL");

            // 清空内置资源目录内容（保留目录）
            string streamingAssetsDir = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
            ClearDirectoryContents(streamingAssetsDir, "StreamingAssets资源");
        }

        /// <summary>
        /// 清空目录内容但保留目录结构
        /// </summary>
        /// <param name="directoryPath">要清空的目录路径</param>
        /// <param name="dirName">目录名称（用于日志显示）</param>
        private static void ClearDirectoryContents(string directoryPath, string dirName)
        {
            if (!Directory.Exists(directoryPath))
            {
                Log.Info($"{dirName}目录不存在，跳过清理: {directoryPath}");
                return;
            }

            try
            {
                // 清空所有文件
                var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    File.SetAttributes(file, FileAttributes.Normal); // 确保文件可删除
                    File.Delete(file);
                }

                // 清空所有子目录
                var directories = Directory.GetDirectories(directoryPath, "*", SearchOption.AllDirectories)
                    .OrderByDescending(d => d.Length); // 从最深层开始删除
                foreach (var dir in directories)
                {
                    if (Directory.Exists(dir))
                    {
                        Directory.Delete(dir, false); // 只删除空目录
                    }
                }

                Log.Info($"已清空{dirName}目录内容: {directoryPath}");
            }
            catch (Exception ex)
            {
                Log.Error($"清空{dirName}目录失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 管理GUI日志Reporter组件
        /// 根据是否启用GUI日志来决定在首场景中添加或删除Reporter组件
        /// </summary>
        /// <param name="enableGuiLog">true: 启用GUI日志，false: 禁用GUI日志</param>
        public static void ManageReporter(bool enableGuiLog)
        {
            // 获取构建设置中的第一个场景
            var buildScenes = EditorBuildSettings.scenes;
            if (buildScenes == null || buildScenes.Length == 0)
            {
                Log.Warn("构建设置中没有场景，无法管理Reporter组件");
                return;
            }

            string firstScenePath = buildScenes[0].path;
            if (string.IsNullOrEmpty(firstScenePath) || !buildScenes[0].enabled)
            {
                Log.Warn("构建设置中第一个场景无效，无法管理Reporter组件");
                return;
            }

            Log.Info($"开始管理Reporter组件，场景: {firstScenePath}");

            // 保存当前场景状态
            var currentScene = EditorSceneManager.GetActiveScene();
            bool needRestoreScene = false;

            try
            {
                // 打开目标场景
                var targetScene = EditorSceneManager.OpenScene(firstScenePath, OpenSceneMode.Single);
                if (targetScene != currentScene)
                    needRestoreScene = true;

                // 查找场景中是否已存在Reporter组件
                Reporter existingReporter = Object.FindObjectOfType<Reporter>();

                if (enableGuiLog)
                {
                    // 启用GUI日志：如果没有Reporter则创建
                    if (existingReporter == null)
                    {
                        Log.Info("启用GUI日志：场景中未找到Reporter组件，开始创建...");
                        ReporterEditor.CreateReporter();
                        Log.Info("Reporter组件创建完成");

                        // 保存场景
                        EditorSceneManager.SaveScene(targetScene);
                        Log.Info("场景保存完成");
                    }
                    else
                    {
                        Log.Info("启用GUI日志：场景中已存在Reporter组件，无需创建");
                    }
                }
                else
                {
                    // 禁用GUI日志：如果存在Reporter则删除
                    if (existingReporter != null)
                    {
                        Log.Info("禁用GUI日志：场景中找到Reporter组件，开始删除...");

                        // 注册Undo操作
                        Undo.DestroyObjectImmediate(existingReporter.gameObject);

                        Log.Info("Reporter组件删除完成");

                        // 保存场景
                        EditorSceneManager.SaveScene(targetScene);
                        Log.Info("场景保存完成");
                    }
                    else
                    {
                        Log.Info("禁用GUI日志：场景中未找到Reporter组件，无需删除");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"管理Reporter组件时发生错误: {ex.Message}");
            }
            finally
            {
                // 恢复原始场景
                if (needRestoreScene && currentScene.IsValid())
                {
                    try
                    {
                        EditorSceneManager.OpenScene(currentScene.path, OpenSceneMode.Single);
                    }
                    catch (System.Exception ex)
                    {
                        Log.Warn($"恢复原始场景失败: {ex.Message}");
                    }
                }
            }
        }
    }

}

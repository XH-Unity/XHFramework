using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace XHFramework.Editor {

/// <summary>
/// 构建工具面板 - 继承自BaseToolPanel
/// </summary>
public class BuildToolPanel : BaseToolPanel
{
    public override string PanelName => "打包构建";
    public override string PanelIcon => "📦";
    public override string Description => "Unity项目构建管理工具，支持离线包、热更新包等多种构建方式";

    // 面板状态
    private bool _showBuildSettings = true;
    private bool _showOfflineBuilds = true;
    private bool _showHotfixBuilds = true;

    private BuildToolSettings _settings;

    // 获取项目根目录
    public static string ProjectRoot => DataTableToolSettings.ProjectRoot;

    // 静态代理属性，保持外部引用兼容
    private static BuildToolSettings Settings => BuildToolSettings.GetOrCreate();
    public static string AotDllDir => Settings.AotDllAbsolutePath;
    public static string JitDllDir => Settings.JitDllAbsolutePath;
    public static string AotDllsString => Settings.aotDllsString;
    public static string JitDllsString => Settings.jitDllsString;
    public static bool EnableLog => Settings.enableLog;
    public static string BuildLogsDir => Settings.BuildLogsAbsolutePath;
    public static string ApkOutputDir => Settings.ApkOutputAbsolutePath;
    public static string iOSOutputDir => Settings.IOSOutputAbsolutePath;

    // 当前选择的构建平台
    private int _selectedPlatformIndex = 0;
    private readonly string[] _platformOptions = { "Android", "iOS" };

    public override void OnEnable()
    {
        _settings = BuildToolSettings.GetOrCreate();
    }

    public override void OnGUI()
    {
        if (_settings == null)
            _settings = BuildToolSettings.GetOrCreate();

        // 构建设置（移到最上面）
        _showBuildSettings = DrawFoldoutGroup("⚙️ 构建设置", _showBuildSettings, DrawBuildSettings);

        GUILayout.Space(10);

        // 构建状态概览
        DrawBuildStatusOverview();

        GUILayout.Space(10);

        // 离线包构建
        _showOfflineBuilds = DrawFoldoutGroup("💿 离线包构建", _showOfflineBuilds, DrawOfflineBuilds);

        // 热更新包构建
        _showHotfixBuilds = DrawFoldoutGroup("🔥 热更新包构建", _showHotfixBuilds, DrawHotfixBuilds);
    }

    /// <summary>
    /// 绘制构建状态概览
    /// </summary>
    private void DrawBuildStatusOverview()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("📊 构建状态概览", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        // 左侧信息
        EditorGUILayout.BeginVertical();
        try
        {
            GUILayout.Label($"当前平台：{EditorUserBuildSettings.activeBuildTarget}", EditorStyles.miniLabel);
            GUILayout.Label($"构建模式：{(_settings.enableLog ? "开发模式" : "发布模式")}", EditorStyles.miniLabel);
        }
        catch (System.Exception e)
        {
            GUILayout.Label($"状态获取失败: {e.Message}", EditorStyles.miniLabel);
        }

        EditorGUILayout.EndVertical();

        // 右侧按钮
        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("📁 打开构建目录", GUILayout.Width(120)))
        {
            OpenBuildDirectory();
        }

        if (GUILayout.Button("📦 打开AB包目录", GUILayout.Width(120)))
        {
            OpenABPackagesDirectory();
        }

        if (GUILayout.Button("📝 打开日志目录", GUILayout.Width(120)))
        {
            OpenBuildLogsDirectory();
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 绘制构建设置
    /// </summary>
    private void DrawBuildSettings()
    {
        EditorGUILayout.BeginVertical("box");

        // 路径设置
        GUILayout.Label("📁 路径设置", EditorStyles.boldLabel);

        DrawPathField("AOTDLL目录:", _settings.AotDllAbsolutePath, true, path =>
        {
            _settings.aotDllDir = DataTableToolSettings.ToRelativePath(path);
            _settings.Save();
        });
        DrawPathField("JITDLL目录:", _settings.JitDllAbsolutePath, true, path =>
        {
            _settings.jitDllDir = DataTableToolSettings.ToRelativePath(path);
            _settings.Save();
        });
        DrawPathField("APK输出目录:", _settings.ApkOutputAbsolutePath, true, path =>
        {
            _settings.apkOutputDir = DataTableToolSettings.ToRelativePath(path);
            _settings.Save();
        });
        DrawPathField("iOS输出目录:", _settings.IOSOutputAbsolutePath, true, path =>
        {
            _settings.iOSOutputDir = DataTableToolSettings.ToRelativePath(path);
            _settings.Save();
        });
        DrawPathField("构建日志目录:", _settings.BuildLogsAbsolutePath, true, path =>
        {
            _settings.buildLogsDir = DataTableToolSettings.ToRelativePath(path);
            _settings.Save();
        });

        GUILayout.Space(10);

        // 编译符号设置
        GUILayout.Label("🔧 编译符号", EditorStyles.boldLabel);

        GUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("当前符号:", GUILayout.Width(100));
        var symbols =
            PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        GUILayout.Label(string.IsNullOrEmpty(symbols) ? "无" : symbols, EditorStyles.helpBox,
            GUILayout.ExpandWidth(true));
        GUILayout.Space(30);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        // Enable Log 切换
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("启用日志:", GUILayout.Width(86));
        EditorGUI.BeginChangeCheck();
        _settings.enableLog = EditorGUILayout.ToggleLeft(_settings.enableLog ? "✅ 已启用" : "❌ 已禁用", _settings.enableLog, GUILayout.ExpandWidth(true));
        if (EditorGUI.EndChangeCheck())
        {
            _settings.Save();
        }

        GUILayout.Space(30);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // DLL列表设置
        GUILayout.Label("📚 DLL列表", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("AOT DLL列表:", GUILayout.Width(100));
        EditorGUI.BeginChangeCheck();
        _settings.aotDllsString = GUILayout.TextField(_settings.aotDllsString, EditorStyles.textField, GUILayout.ExpandWidth(true));
        if (EditorGUI.EndChangeCheck())
        {
            _settings.Save();
        }
        GUILayout.Space(30);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("JIT DLL列表:", GUILayout.Width(100));
        EditorGUI.BeginChangeCheck();
        _settings.jitDllsString = GUILayout.TextField(_settings.jitDllsString, EditorStyles.textField, GUILayout.ExpandWidth(true));
        if (EditorGUI.EndChangeCheck())
        {
            _settings.Save();
        }
        GUILayout.Space(30);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);


        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 绘制路径字段（带文件夹选择器）
    /// </summary>
    private void DrawPathField(string label, string currentAbsPath, bool isFolder, System.Action<string> onPathChanged)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(100));

        string displayPath = string.IsNullOrEmpty(currentAbsPath) ? "未设置" : currentAbsPath;
        GUILayout.Label(displayPath, EditorStyles.helpBox, GUILayout.ExpandWidth(true));

        if (GUILayout.Button("📂", GUILayout.Width(30)))
        {
            string defaultDir = Directory.Exists(currentAbsPath)
                ? currentAbsPath
                : (string.IsNullOrEmpty(currentAbsPath) ? Application.dataPath : System.IO.Path.GetDirectoryName(currentAbsPath));

            string selectedPath = "";
            if (isFolder)
            {
                selectedPath = EditorUtility.OpenFolderPanel($"选择{label}", defaultDir, "");
            }
            else
            {
                string extension = System.IO.Path.GetExtension(currentAbsPath);
                selectedPath = EditorUtility.OpenFilePanel($"选择{label}", defaultDir, extension?.TrimStart('.') ?? "");
            }

            if (!string.IsNullOrEmpty(selectedPath))
            {
                onPathChanged?.Invoke(selectedPath);
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 绘制离线包构建
    /// </summary>
    private void DrawOfflineBuilds()
    {
        // 平台选择
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("目标平台:", GUILayout.Width(70));
        _selectedPlatformIndex = GUILayout.Toolbar(_selectedPlatformIndex, _platformOptions, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        if (_selectedPlatformIndex == 0) // Android
        {
            DrawButtonGroup(
                "Android离线包",
                "",
                new ButtonInfo("📱 构建全量包(离线)",
                    () => { EditorApplication.delayCall += () => { BuildPipelineEditor.BuildOfflineAPK(); }; }, null, true,
                    35)
            );
        }
        else // iOS
        {
            DrawButtonGroup(
                "iOS离线包",
                "",
                new ButtonInfo("🍎 构建全量包(离线)",
                    () => { EditorApplication.delayCall += () => { BuildPipelineEditor.BuildOfflineiOS(); }; }, null, true,
                    35)
            );
        }
    }

    /// <summary>
    /// 绘制热更新包构建
    /// </summary>
    private void DrawHotfixBuilds()
    {
        if (_selectedPlatformIndex == 0) // Android
        {
            DrawButtonGroup(
                "Android基础包构建",
                "",
                new ButtonInfo("📦 构建全量包APK(热更)",
                    () => { EditorApplication.delayCall += () => { BuildPipelineEditor.BuildFullPackageAPK(); }; }, null,
                    true, 35),
                new ButtonInfo("🗃️ 构建空包APK(热更)",
                    () => { EditorApplication.delayCall += () => { BuildPipelineEditor.BuildNulllPackageAPK(); }; }, null,
                    true, 35)
            );
        }
        else // iOS
        {
            DrawButtonGroup(
                "iOS基础包构建",
                "",
                new ButtonInfo("📦 构建全量包iOS(热更)",
                    () => { EditorApplication.delayCall += () => { BuildPipelineEditor.BuildFullPackageiOS(); }; }, null,
                    true, 35),
                new ButtonInfo("🗃️ 构建空包iOS(热更)",
                    () => { EditorApplication.delayCall += () => { BuildPipelineEditor.BuildNullPackageiOS(); }; }, null,
                    true, 35)
            );
        }

        GUILayout.Space(8);

        DrawButtonGroup(
            "增量更新包",
            "",
            new ButtonInfo("🔄 构建增量包",
                () => { EditorApplication.delayCall += () => { BuildPipelineEditor.BuildIncrementalPackageNoAPK(); }; },
                null, true, 35)
        );
    }

    #region 私有方法

    /// <summary>
    /// 直接打开目录
    /// </summary>
    private void OpenDirectoryDirectly(string path)
    {
        string fullPath = System.IO.Path.GetFullPath(path);
        System.Diagnostics.Process.Start("explorer.exe", fullPath);
    }

    private void OpenBuildDirectory()
    {
        string buildPath = _settings.ApkOutputAbsolutePath;
        if (Directory.Exists(buildPath))
        {
            OpenDirectoryDirectly(buildPath);
        }
        else
        {
            if (EditorUtility.DisplayDialog("提示", $"构建目录不存在:\n{buildPath}\n\n是否创建该目录？", "创建", "取消"))
            {
                try
                {
                    Directory.CreateDirectory(buildPath);
                    OpenDirectoryDirectly(buildPath);
                }
                catch (System.Exception e)
                {
                    EditorUtility.DisplayDialog("错误", $"创建构建目录失败: {e.Message}", "确定");
                }
            }
        }
    }

    private void OpenABPackagesDirectory()
    {
        string abPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../Bundles"));
        if (Directory.Exists(abPath))
        {
            OpenDirectoryDirectly(abPath);
        }
        else
        {
            string[] possiblePaths =
            {
                System.IO.Path.Combine(Application.dataPath, "../AssetBundles"),
                System.IO.Path.Combine(Application.dataPath, "../StreamingAssets"),
                System.IO.Path.Combine(Application.streamingAssetsPath, "")
            };

            foreach (string path in possiblePaths)
            {
                if (Directory.Exists(path))
                {
                    OpenDirectoryDirectly(path);
                    return;
                }
            }

            EditorUtility.DisplayDialog("提示",
                "AB包目录不存在，可能的路径:\n- Bundles\n- AssetBundles\n- StreamingAssets\n\n请先执行资源包构建操作", "确定");
        }
    }

    private void OpenBuildLogsDirectory()
    {
        string logsPath = _settings.BuildLogsAbsolutePath;
        if (Directory.Exists(logsPath))
        {
            OpenDirectoryDirectly(logsPath);
        }
        else
        {
            if (EditorUtility.DisplayDialog("提示", $"日志目录不存在:\n{logsPath}\n\n是否创建该目录？", "创建", "取消"))
            {
                try
                {
                    Directory.CreateDirectory(logsPath);
                    OpenDirectoryDirectly(logsPath);
                }
                catch (System.Exception e)
                {
                    EditorUtility.DisplayDialog("错误", $"创建日志目录失败: {e.Message}", "确定");
                }
            }
        }
    }



    public static List<string> GetAotDLLNames()
    {
        return Settings.GetAotDLLNames();
    }

    public static List<string> GetJITDLLNames()
    {
        return Settings.GetJITDLLNames();
    }

    #endregion
}

}

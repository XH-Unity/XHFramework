using UnityEngine;
using UnityEditor;
using System.Diagnostics;

namespace XHFramework.Editor {

/// <summary>
/// 配置表工具面板
/// </summary>
public class DataTableToolPanel : BaseToolPanel
{
    public override string PanelName => "配置表工具";
    public override string PanelIcon => "📋";
    public override string Description => "配置表导入和管理工具";

    private DataTableToolSettings _settings;

    public override void OnEnable()
    {
        _settings = DataTableToolSettings.GetOrCreate();
    }

    public override void OnGUI()
    {
        DrawHeader();

        EditorGUILayout.HelpBox(Description, MessageType.Info);
        GUILayout.Space(10);

        if (_settings == null)
            _settings = DataTableToolSettings.GetOrCreate();

        // 绘制配置表路径信息
        DrawDataTablePaths();

        GUILayout.Space(10);

        // 绘制操作按钮
        DrawOperationButtons();
    }

    /// <summary>
    /// 绘制配置表路径信息
    /// </summary>
    private void DrawDataTablePaths()
    {
        // 基础配置
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("基础配置", EditorStyles.boldLabel);
        GUILayout.Space(5);

        DrawPathField("导入脚本:", _settings.GenBatAbsolutePath, "bat", "选择配置表导入脚本", false, path =>
        {
            _settings.genBatPath = DataTableToolSettings.ToRelativePath(path);
            _settings.Save();
        });

        DrawPathField("数据目录:", _settings.DataTableDataAbsolutePath, null, "选择配置表数据目录", true, path =>
        {
            _settings.dataTableDataPath = DataTableToolSettings.ToRelativePath(path);
            _settings.Save();
        });

        EditorGUILayout.EndVertical();

        GUILayout.Space(5);

        // 输出路径配置（修改后同步到 gen.bat）
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("输出路径配置", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("修改后会自动更新 gen.bat 中的输出路径", MessageType.Info);
        GUILayout.Space(5);

        DrawPathField("数据输出:", _settings.OutputDataAbsolutePath, null, "选择配置表数据输出目录", true, path =>
        {
            _settings.outputDataDir = DataTableToolSettings.ToRelativePath(path);
            _settings.Save();
            UpdateBatOutputPaths();
        });

        DrawPathField("代码输出:", _settings.OutputCodeAbsolutePath, null, "选择配置表代码输出目录", true, path =>
        {
            _settings.outputCodeDir = DataTableToolSettings.ToRelativePath(path);
            _settings.Save();
            UpdateBatOutputPaths();
        });

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 绘制路径字段（带选择按钮）
    /// </summary>
    private void DrawPathField(string label, string currentAbsPath, string extension, string dialogTitle, bool isFolder, System.Action<string> onPathChanged)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(70));
        EditorGUILayout.SelectableLabel(currentAbsPath, EditorStyles.textField, GUILayout.Height(18));

        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            string defaultDir = System.IO.Directory.Exists(currentAbsPath)
                ? currentAbsPath
                : System.IO.Path.GetDirectoryName(currentAbsPath);

            string selectedPath;
            if (isFolder)
            {
                selectedPath = EditorUtility.OpenFolderPanel(dialogTitle, defaultDir, "");
            }
            else
            {
                selectedPath = EditorUtility.OpenFilePanel(dialogTitle, defaultDir, extension);
            }

            if (!string.IsNullOrEmpty(selectedPath))
            {
                onPathChanged?.Invoke(selectedPath);
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 更新 bat 文件中的输出路径配置
    /// </summary>
    private void UpdateBatOutputPaths()
    {
        string genBatAbsPath = _settings.GenBatAbsolutePath;

        if (!System.IO.File.Exists(genBatAbsPath))
        {
            UnityEngine.Debug.LogWarning($"无法更新导入脚本，文件不存在: {genBatAbsPath}");
            return;
        }

        try
        {
            // 计算相对于 gen.bat 所在目录的相对路径
            string genBatDir = System.IO.Path.GetDirectoryName(genBatAbsPath);
            string outputDataRelPath = DataTableToolSettings.ToRelativePath(_settings.OutputDataAbsolutePath, genBatDir);
            string outputCodeRelPath = DataTableToolSettings.ToRelativePath(_settings.OutputCodeAbsolutePath, genBatDir);

            string[] lines = System.IO.File.ReadAllLines(genBatAbsPath);
            bool modified = false;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                // 查找并替换 outputDataDir
                if (line.Contains("-x outputDataDir="))
                {
                    int indentEnd = 0;
                    while (indentEnd < line.Length && char.IsWhiteSpace(line[indentEnd]))
                        indentEnd++;
                    string indent = line.Substring(0, indentEnd);

                    lines[i] = $"{indent}-x outputDataDir={outputDataRelPath.Replace("/", "\\")} ^";
                    modified = true;
                }
                // 查找并替换 outputCodeDir
                else if (line.Contains("-x outputCodeDir="))
                {
                    int indentEnd = 0;
                    while (indentEnd < line.Length && char.IsWhiteSpace(line[indentEnd]))
                        indentEnd++;
                    string indent = line.Substring(0, indentEnd);

                    lines[i] = $"{indent}-x outputCodeDir={outputCodeRelPath.Replace("/", "\\")} ^";
                    modified = true;
                }
            }

            if (modified)
            {
                System.IO.File.WriteAllLines(genBatAbsPath, lines);
                UnityEngine.Debug.Log($"已更新导入脚本的输出路径配置:\n数据输出: {outputDataRelPath}\n代码输出: {outputCodeRelPath}");
            }
            else
            {
                UnityEngine.Debug.LogWarning("未找到 outputDataDir 或 outputCodeDir 配置项");
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"更新导入脚本失败: {e.Message}");
            EditorUtility.DisplayDialog("错误", $"更新导入脚本失败:\n{e.Message}", "确定");
        }
    }

    /// <summary>
    /// 绘制操作按钮
    /// </summary>
    private void DrawOperationButtons()
    {
        DrawButtonGroup(
            "配置表操作",
            "执行配置表导入或打开配置表文件夹",
            new ButtonInfo("🔄 执行配置表导入", ExecuteGenBat, null, false, 40),
            new ButtonInfo("📂 打开配置表文件夹", OpenDataTableFolder, null, false, 40)
        );
    }

    /// <summary>
    /// 执行配置表导入 bat 文件
    /// </summary>
    private void ExecuteGenBat()
    {
        string genBatAbsPath = _settings.GenBatAbsolutePath;

        if (!System.IO.File.Exists(genBatAbsPath))
        {
            EditorUtility.DisplayDialog("错误", $"找不到配置表导入工具:\n{genBatAbsPath}", "确定");
            return;
        }

        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = genBatAbsPath;
            startInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(genBatAbsPath) ?? "";
            startInfo.UseShellExecute = true;

            Process.Start(startInfo);

            AssetDatabase.Refresh();
            UnityEngine.Debug.Log("配置表导入工具已启动");
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"执行配置表导入工具失败: {e.Message}");
        }
    }

    /// <summary>
    /// 打开配置表文件夹
    /// </summary>
    private void OpenDataTableFolder()
    {
        string dataPath = _settings.DataTableDataAbsolutePath;

        if (!System.IO.Directory.Exists(dataPath))
        {
            EditorUtility.DisplayDialog("错误", $"找不到配置表文件夹:\n{dataPath}", "确定");
            return;
        }

        try
        {
            string windowsPath = dataPath.Replace("/", "\\");
            Process.Start("explorer.exe", windowsPath);
            UnityEngine.Debug.Log($"已打开配置表文件夹: {dataPath}");
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"打开配置表文件夹失败: {e.Message}");
            EditorUtility.DisplayDialog("错误", $"打开配置表文件夹失败:\n{e.Message}", "确定");
        }
    }
}

}

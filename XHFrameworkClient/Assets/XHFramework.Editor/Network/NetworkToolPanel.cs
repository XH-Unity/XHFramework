using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace XHFramework.Editor {

/// <summary>
/// 网络模块工具面板 - Proto文件导入工具
/// </summary>
public class NetworkToolPanel : BaseToolPanel
{
    public override string PanelName => "网络模块工具";
    public override string PanelIcon => "🌐";
    public override string Description => "Proto文件导入和管理工具";

    private NetworkToolSettings _settings;

    private bool tcpFoldout = true;
    private bool udpFoldout = true;
    private bool webSocketFoldout = true;
    private Vector2 scrollPosition;

    public override void OnEnable()
    {
        _settings = NetworkToolSettings.GetOrCreate();
    }

    public override void OnGUI()
    {
        DrawHeader();

        EditorGUILayout.HelpBox(Description, MessageType.Info);
        GUILayout.Space(10);

        if (_settings == null)
            _settings = NetworkToolSettings.GetOrCreate();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // 绘制 Protoc 路径信息
        DrawProtocInfo();

        GUILayout.Space(10);

        // 绘制 TCP Proto 操作区域
        tcpFoldout = DrawFoldoutGroup("TCP Proto", tcpFoldout, DrawTcpProtoSection);

        GUILayout.Space(10);

        // 绘制 UDP Proto 操作区域
        udpFoldout = DrawFoldoutGroup("UDP Proto", udpFoldout, DrawUdpProtoSection);

        GUILayout.Space(10);

        // 绘制 WebSocket Proto 操作区域
        webSocketFoldout = DrawFoldoutGroup("WebSocket Proto", webSocketFoldout, DrawWebSocketProtoSection);

        GUILayout.Space(10);

        // 绘制批量操作按钮
        DrawBatchOperations();

        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// 绘制 Protoc 编译器信息
    /// </summary>
    private void DrawProtocInfo()
    {
        EditorGUILayout.BeginVertical("box");

        GUILayout.Label("Protoc 编译器", EditorStyles.boldLabel);
        GUILayout.Space(5);

        DrawPathField("编译器路径:", _settings.ProtocAbsolutePath, "exe", "选择 Protoc 编译器", false, path =>
        {
            _settings.protocPath = DataTableToolSettings.ToRelativePath(path);
            _settings.Save();
        });

        bool protocExists = File.Exists(_settings.ProtocAbsolutePath);
        if (protocExists)
        {
            EditorGUILayout.HelpBox("Protoc 编译器已就绪", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("找不到 Protoc 编译器，请检查路径是否正确", MessageType.Error);
        }

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 绘制 TCP Proto 操作区域
    /// </summary>
    private void DrawTcpProtoSection()
    {
        DrawPathField("Proto 源目录:", _settings.TcpProtoSourceAbsolutePath, null, "选择 TCP Proto 源目录", true, path =>
        {
            _settings.tcpProtoSourcePath = DataTableToolSettings.ToRelativePath(path);
            _settings.Save();
        });
        DrawPathField("输出目录:", _settings.TcpProtoOutputAbsolutePath, null, "选择 TCP 输出目录", true, path =>
        {
            _settings.tcpProtoOutputPath = DataTableToolSettings.ToRelativePath(path);
            _settings.Save();
        });

        GUILayout.Space(5);

        DrawButtonGroup(
            "TCP Proto 操作",
            "编译 TCP 协议文件或打开相关文件夹",
            new ButtonInfo("🔄 编译 TCP Proto", () => CompileProto(_settings.TcpProtoSourceAbsolutePath, _settings.TcpProtoOutputAbsolutePath, "TCP"), null, false, 35),
            new ButtonInfo("📂 打开 TCP Proto 文件夹", () => OpenFolder(_settings.TcpProtoSourceAbsolutePath), null, false, 35),
            new ButtonInfo("📂 打开 TCP 输出文件夹", () => OpenFolder(_settings.TcpProtoOutputAbsolutePath), null, false, 35),
            new ButtonInfo("🗑 清空 TCP 输出", () => ClearOutputFolder(_settings.TcpProtoOutputAbsolutePath, "TCP"), "确定要清空 TCP 输出文件夹吗？", true, 35)
        );
    }

    /// <summary>
    /// 绘制 UDP Proto 操作区域
    /// </summary>
    private void DrawUdpProtoSection()
    {
        DrawPathField("Proto 源目录:", _settings.UdpProtoSourceAbsolutePath, null, "选择 UDP Proto 源目录", true, path =>
        {
            _settings.udpProtoSourcePath = DataTableToolSettings.ToRelativePath(path);
            _settings.Save();
        });
        DrawPathField("输出目录:", _settings.UdpProtoOutputAbsolutePath, null, "选择 UDP 输出目录", true, path =>
        {
            _settings.udpProtoOutputPath = DataTableToolSettings.ToRelativePath(path);
            _settings.Save();
        });

        GUILayout.Space(5);

        DrawButtonGroup(
            "UDP Proto 操作",
            "编译 UDP 协议文件或打开相关文件夹",
            new ButtonInfo("🔄 编译 UDP Proto", () => CompileProto(_settings.UdpProtoSourceAbsolutePath, _settings.UdpProtoOutputAbsolutePath, "UDP"), null, false, 35),
            new ButtonInfo("📂 打开 UDP Proto 文件夹", () => OpenFolder(_settings.UdpProtoSourceAbsolutePath), null, false, 35),
            new ButtonInfo("📂 打开 UDP 输出文件夹", () => OpenFolder(_settings.UdpProtoOutputAbsolutePath), null, false, 35),
            new ButtonInfo("🗑 清空 UDP 输出", () => ClearOutputFolder(_settings.UdpProtoOutputAbsolutePath, "UDP"), "确定要清空 UDP 输出文件夹吗？", true, 35)
        );
    }

    /// <summary>
    /// 绘制 WebSocket Proto 操作区域
    /// </summary>
    private void DrawWebSocketProtoSection()
    {
        DrawPathField("Proto 源目录:", _settings.WebSocketProtoSourceAbsolutePath, null, "选择 WebSocket Proto 源目录", true, path =>
        {
            _settings.webSocketProtoSourcePath = DataTableToolSettings.ToRelativePath(path);
            _settings.Save();
        });
        DrawPathField("输出目录:", _settings.WebSocketProtoOutputAbsolutePath, null, "选择 WebSocket 输出目录", true, path =>
        {
            _settings.webSocketProtoOutputPath = DataTableToolSettings.ToRelativePath(path);
            _settings.Save();
        });

        GUILayout.Space(5);

        DrawButtonGroup(
            "WebSocket Proto 操作",
            "编译 WebSocket 协议文件或打开相关文件夹",
            new ButtonInfo("🔄 编译 WebSocket Proto", () => CompileProto(_settings.WebSocketProtoSourceAbsolutePath, _settings.WebSocketProtoOutputAbsolutePath, "WebSocket"), null, false, 35),
            new ButtonInfo("📂 打开 WebSocket Proto 文件夹", () => OpenFolder(_settings.WebSocketProtoSourceAbsolutePath), null, false, 35),
            new ButtonInfo("📂 打开 WebSocket 输出文件夹", () => OpenFolder(_settings.WebSocketProtoOutputAbsolutePath), null, false, 35),
            new ButtonInfo("🗑 清空 WebSocket 输出", () => ClearOutputFolder(_settings.WebSocketProtoOutputAbsolutePath, "WebSocket"), "确定要清空 WebSocket 输出文件夹吗？", true, 35)
        );
    }

    /// <summary>
    /// 绘制批量操作按钮
    /// </summary>
    private void DrawBatchOperations()
    {
        DrawButtonGroup(
            "批量操作",
            "一键编译所有 Proto 文件",
            new ButtonInfo("🔄 编译所有 Proto", CompileAllProto, null, false, 45),
            new ButtonInfo("🗑 清空所有输出", ClearAllOutput, "确定要清空所有输出文件夹吗？", true, 45)
        );
    }

    /// <summary>
    /// 绘制路径字段（带选择按钮）
    /// </summary>
    private void DrawPathField(string label, string currentAbsPath, string extension, string dialogTitle, bool isFolder, System.Action<string> onPathChanged)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(80));
        EditorGUILayout.SelectableLabel(currentAbsPath, EditorStyles.textField, GUILayout.Height(18));

        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            string defaultDir = Directory.Exists(currentAbsPath)
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
    /// 编译 Proto 文件
    /// </summary>
    private void CompileProto(string sourcePath, string outputPath, string protoType)
    {
        string protocAbsPath = _settings.ProtocAbsolutePath;

        if (!File.Exists(protocAbsPath))
        {
            EditorUtility.DisplayDialog("错误", $"找不到 Protoc 编译器:\n{protocAbsPath}", "确定");
            return;
        }

        if (!Directory.Exists(sourcePath))
        {
            EditorUtility.DisplayDialog("错误", $"找不到 {protoType} Proto 源目录:\n{sourcePath}", "确定");
            return;
        }

        // 确保输出目录存在
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
            UnityEngine.Debug.Log($"创建输出目录: {outputPath}");
        }

        string[] protoFiles = Directory.GetFiles(sourcePath, "*.proto");
        if (protoFiles.Length == 0)
        {
            EditorUtility.DisplayDialog("提示", $"{protoType} Proto 目录中没有 .proto 文件", "确定");
            return;
        }

        int successCount = 0;
        int failCount = 0;
        List<string> errorMessages = new List<string>();

        EditorUtility.DisplayProgressBar($"编译 {protoType} Proto", "正在编译...", 0f);

        try
        {
            for (int i = 0; i < protoFiles.Length; i++)
            {
                string protoFile = protoFiles[i];
                string fileName = Path.GetFileName(protoFile);

                EditorUtility.DisplayProgressBar($"编译 {protoType} Proto", $"正在编译: {fileName}", (float)i / protoFiles.Length);

                if (CompileSingleProto(protoFile, sourcePath, outputPath, out string error))
                {
                    successCount++;
                    UnityEngine.Debug.Log($"编译成功: {fileName}");
                }
                else
                {
                    failCount++;
                    errorMessages.Add($"{fileName}: {error}");
                    UnityEngine.Debug.LogError($"编译失败: {fileName}\n{error}");
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.Refresh();

        string resultMessage = $"{protoType} Proto 编译完成\n成功: {successCount} 个\n失败: {failCount} 个";
        if (errorMessages.Count > 0)
        {
            resultMessage += "\n\n错误详情:\n" + string.Join("\n", errorMessages);
        }

        EditorUtility.DisplayDialog("编译结果", resultMessage, "确定");
        UnityEngine.Debug.Log(resultMessage);
    }

    /// <summary>
    /// 编译单个 Proto 文件
    /// </summary>
    private bool CompileSingleProto(string protoFile, string protoPath, string outputPath, out string error)
    {
        error = null;

        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = _settings.ProtocAbsolutePath,
                Arguments = $"--csharp_out=\"{outputPath}\" --proto_path=\"{protoPath}\" \"{protoFile}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = protoPath
            };

            using (Process process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                string errorOutput = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    error = string.IsNullOrEmpty(errorOutput) ? "未知错误" : errorOutput;
                    return false;
                }

                return true;
            }
        }
        catch (System.Exception e)
        {
            error = e.Message;
            return false;
        }
    }

    /// <summary>
    /// 编译所有 Proto 文件
    /// </summary>
    private void CompileAllProto()
    {
        UnityEngine.Debug.Log("开始编译所有 Proto 文件...");

        if (Directory.Exists(_settings.TcpProtoSourceAbsolutePath))
            CompileProto(_settings.TcpProtoSourceAbsolutePath, _settings.TcpProtoOutputAbsolutePath, "TCP");

        if (Directory.Exists(_settings.UdpProtoSourceAbsolutePath))
            CompileProto(_settings.UdpProtoSourceAbsolutePath, _settings.UdpProtoOutputAbsolutePath, "UDP");

        if (Directory.Exists(_settings.WebSocketProtoSourceAbsolutePath))
            CompileProto(_settings.WebSocketProtoSourceAbsolutePath, _settings.WebSocketProtoOutputAbsolutePath, "WebSocket");

        UnityEngine.Debug.Log("所有 Proto 文件编译完成");
    }

    /// <summary>
    /// 打开文件夹
    /// </summary>
    private void OpenFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            if (EditorUtility.DisplayDialog("文件夹不存在", $"文件夹不存在:\n{folderPath}\n\n是否创建该文件夹？", "创建", "取消"))
            {
                try
                {
                    Directory.CreateDirectory(folderPath);
                    UnityEngine.Debug.Log($"创建文件夹: {folderPath}");
                }
                catch (System.Exception e)
                {
                    EditorUtility.DisplayDialog("错误", $"创建文件夹失败:\n{e.Message}", "确定");
                    return;
                }
            }
            else
            {
                return;
            }
        }

        try
        {
            string windowsPath = folderPath.Replace("/", "\\");
            Process.Start("explorer.exe", windowsPath);
            UnityEngine.Debug.Log($"已打开文件夹: {folderPath}");
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"打开文件夹失败: {e.Message}");
            EditorUtility.DisplayDialog("错误", $"打开文件夹失败:\n{e.Message}", "确定");
        }
    }

    /// <summary>
    /// 清空输出文件夹
    /// </summary>
    private void ClearOutputFolder(string outputPath, string protoType)
    {
        if (!Directory.Exists(outputPath))
        {
            EditorUtility.DisplayDialog("提示", $"{protoType} 输出文件夹不存在", "确定");
            return;
        }

        try
        {
            string[] csFiles = Directory.GetFiles(outputPath, "*.cs");
            int deletedCount = 0;

            foreach (string file in csFiles)
            {
                File.Delete(file);
                deletedCount++;
            }

            string[] metaFiles = Directory.GetFiles(outputPath, "*.cs.meta");
            foreach (string file in metaFiles)
            {
                File.Delete(file);
            }

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("完成", $"已删除 {deletedCount} 个 {protoType} 生成文件", "确定");
            UnityEngine.Debug.Log($"已清空 {protoType} 输出文件夹，删除 {deletedCount} 个文件");
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"清空 {protoType} 输出文件夹失败: {e.Message}");
            EditorUtility.DisplayDialog("错误", $"清空输出文件夹失败:\n{e.Message}", "确定");
        }
    }

    /// <summary>
    /// 清空所有输出文件夹
    /// </summary>
    private void ClearAllOutput()
    {
        ClearOutputFolder(_settings.TcpProtoOutputAbsolutePath, "TCP");
        ClearOutputFolder(_settings.UdpProtoOutputAbsolutePath, "UDP");
        ClearOutputFolder(_settings.WebSocketProtoOutputAbsolutePath, "WebSocket");
    }
}

}

using UnityEngine;
using UnityEditor;

namespace XHFramework.Editor {

/// <summary>
/// 配置面板 - YooAsset配置部分
/// </summary>
public partial class ConfigPanel
{
    // YooAsset配置字段
    private bool _allowDownloadOverHTTP = true;
    private bool _hasForceGLESParam = false;

    /// <summary>
    /// 加载YooAsset配置
    /// </summary>
    private void LoadYooAssetSettings()
    {
        _allowDownloadOverHTTP = PlayerSettings.insecureHttpOption == InsecureHttpOption.AlwaysAllowed;
        UpdateForceGLESStatus();
    }

    /// <summary>
    /// 绘制YooAsset配置
    /// </summary>
    private void DrawYooAssetSettings()
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("YooAsset设置", EditorStyles.boldLabel);

        // 配置前提示
        EditorGUILayout.HelpBox(
            "配置前请确保：\n" +
            "1. 安装 YooAsset\n" +
            "2. 创建 YooAsset Setting 文件\n" +
            "3. 设置 YooAsset 资源包配置",
            MessageType.Info);

        GUILayout.Space(5);

        // Allow Download Over HTTP
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Allow Download Over HTTP:", GUILayout.Width(180));
        bool newAllowHTTP = EditorGUILayout.Toggle(_allowDownloadOverHTTP);
        if (newAllowHTTP != _allowDownloadOverHTTP)
        {
            _allowDownloadOverHTTP = newAllowHTTP;
            PlayerSettings.insecureHttpOption = _allowDownloadOverHTTP ?
                InsecureHttpOption.AlwaysAllowed : InsecureHttpOption.NotAllowed;
            AssetDatabase.SaveAssets();
            Debug.Log($"[ConfigPanel] Allow Download Over HTTP 已设置为: {_allowDownloadOverHTTP}");
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // 渲染器模式
        UpdateForceGLESStatus();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("渲染器模式:", GUILayout.Width(180));

        if (!_hasForceGLESParam)
        {
            if (GUILayout.Button("添加 -force-gles 参数", GUILayout.Height(25)))
            {
                AddForceGLESParameter();
            }
        }
        else
        {
            EditorGUILayout.LabelField("-force-gles", GUILayout.Width(100));
            if (GUILayout.Button("移除", GUILayout.Width(60), GUILayout.Height(25)))
            {
                RemoveForceGLESParameter();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 更新Force GLES状态
    /// </summary>
    private void UpdateForceGLESStatus()
    {
        string additionalArgs = EditorPrefs.GetString("UnityEditor.AdditionalCommandlineArguments", "");
        _hasForceGLESParam = additionalArgs.Contains("-force-gles");
    }

    /// <summary>
    /// 添加Force GLES参数
    /// </summary>
    private void AddForceGLESParameter()
    {
        string additionalArgs = EditorPrefs.GetString("UnityEditor.AdditionalCommandlineArguments", "");
        if (!additionalArgs.Contains("-force-gles"))
        {
            additionalArgs = string.IsNullOrEmpty(additionalArgs) ? "-force-gles" : additionalArgs + " -force-gles";
            EditorPrefs.SetString("UnityEditor.AdditionalCommandlineArguments", additionalArgs);
            UpdateForceGLESStatus();
            Debug.Log("[ConfigPanel] 已添加 -force-gles 参数");
        }
    }

    /// <summary>
    /// 移除Force GLES参数
    /// </summary>
    private void RemoveForceGLESParameter()
    {
        string additionalArgs = EditorPrefs.GetString("UnityEditor.AdditionalCommandlineArguments", "");
        if (additionalArgs.Contains("-force-gles"))
        {
            additionalArgs = additionalArgs.Replace("-force-gles", "").Trim();
            EditorPrefs.SetString("UnityEditor.AdditionalCommandlineArguments", additionalArgs);
            UpdateForceGLESStatus();
            Debug.Log("[ConfigPanel] 已移除 -force-gles 参数");
        }
    }
}

}

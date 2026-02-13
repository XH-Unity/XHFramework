using UnityEngine;
using UnityEditor;

namespace XHFramework.Editor {

/// <summary>
/// 配置面板 - HybridCLR配置部分
/// </summary>
public partial class ConfigPanel
{
    // HybridCLR配置字段
    private ScriptingImplementation _scriptingBackend = ScriptingImplementation.IL2CPP;
    private ApiCompatibilityLevel _apiCompatibilityLevel = ApiCompatibilityLevel.NET_Standard_2_0;
    private bool _stripEngineCode = false;
    private ManagedStrippingLevel _managedStrippingLevel = ManagedStrippingLevel.Minimal;

    /// <summary>
    /// 加载HybridCLR配置
    /// </summary>
    private void LoadHybridClrSettings()
    {
        var targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        _scriptingBackend = PlayerSettings.GetScriptingBackend(targetGroup);
        _apiCompatibilityLevel = PlayerSettings.GetApiCompatibilityLevel(targetGroup);
        _stripEngineCode = PlayerSettings.stripEngineCode;
        _managedStrippingLevel = PlayerSettings.GetManagedStrippingLevel(targetGroup);
    }

    /// <summary>
    /// 绘制HybridCLR配置
    /// </summary>
    private void DrawHybridClrSettings()
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("HybridCLR设置", EditorStyles.boldLabel);

        // 配置前提示
        EditorGUILayout.HelpBox(
            "配置前请确保：\n" +
            "1. 安装 IL2CPP 模块\n" +
            "2. 安装 HybridCLR\n" +
            "3. 设置 HybridCLR 热更程序集",
            MessageType.Info);

        GUILayout.Space(5);

        var targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

        // Scripting Backend
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Scripting Backend:", GUILayout.Width(180));
        ScriptingImplementation newBackend = (ScriptingImplementation)EditorGUILayout.EnumPopup(_scriptingBackend);
        if (newBackend != _scriptingBackend)
        {
            _scriptingBackend = newBackend;
            PlayerSettings.SetScriptingBackend(targetGroup, _scriptingBackend);
            AssetDatabase.SaveAssets();
            Debug.Log($"[ConfigPanel] Scripting Backend 已设置为: {_scriptingBackend}");
        }
        EditorGUILayout.EndHorizontal();

        // API Compatibility Level
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("API Compatibility Level:", GUILayout.Width(180));
        ApiCompatibilityLevel newApiLevel = (ApiCompatibilityLevel)EditorGUILayout.EnumPopup(_apiCompatibilityLevel);
        if (newApiLevel != _apiCompatibilityLevel)
        {
            _apiCompatibilityLevel = newApiLevel;
            PlayerSettings.SetApiCompatibilityLevel(targetGroup, _apiCompatibilityLevel);
            AssetDatabase.SaveAssets();
            Debug.Log($"[ConfigPanel] API Compatibility Level 已设置为: {_apiCompatibilityLevel}");
        }
        EditorGUILayout.EndHorizontal();

        // Strip Engine Code
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Strip Engine Code:", GUILayout.Width(180));
        bool newStripEngine = EditorGUILayout.Toggle(_stripEngineCode);
        if (newStripEngine != _stripEngineCode)
        {
            _stripEngineCode = newStripEngine;
            PlayerSettings.stripEngineCode = _stripEngineCode;
            AssetDatabase.SaveAssets();
            Debug.Log($"[ConfigPanel] Strip Engine Code 已设置为: {_stripEngineCode}");
        }
        EditorGUILayout.EndHorizontal();

        // Managed Stripping Level
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Managed Stripping Level:", GUILayout.Width(180));
        ManagedStrippingLevel newStrippingLevel = (ManagedStrippingLevel)EditorGUILayout.EnumPopup(_managedStrippingLevel);
        if (newStrippingLevel != _managedStrippingLevel)
        {
            _managedStrippingLevel = newStrippingLevel;
            PlayerSettings.SetManagedStrippingLevel(targetGroup, _managedStrippingLevel);
            AssetDatabase.SaveAssets();
            Debug.Log($"[ConfigPanel] Managed Stripping Level 已设置为: {_managedStrippingLevel}");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }
}

}

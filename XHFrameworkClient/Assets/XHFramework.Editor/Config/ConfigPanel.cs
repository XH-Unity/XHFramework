using UnityEngine;
using UnityEditor;
namespace XHFramework.Editor {

/// <summary>
/// 项目配置面板 - 主文件
/// </summary>
public partial class ConfigPanel : BaseToolPanel
{
    public override string PanelName => "项目配置";
    public override string PanelIcon => "⚙️";
    public override string Description => "全局项目配置管理";

    // 折叠状态
    private bool _globalFoldout = true;
    private bool _hybridClrFoldout = true;
    private bool _yooAssetFoldout = true;
    private bool _macroDefinesFoldout = true;

    public override void OnEnable()
    {
        LoadCurrentSettings();
    }

    public override void OnGUI()
    {
        DrawHeader();

        EditorGUILayout.BeginVertical();

        // 全局配置
        _globalFoldout = DrawFoldoutGroup("全局配置", _globalFoldout, () =>
        {
            DrawGlobalSettings();
        });

        GUILayout.Space(10);

        // HybridCLR配置
        _hybridClrFoldout = DrawFoldoutGroup("HybridCLR配置", _hybridClrFoldout, () =>
        {
            DrawHybridClrSettings();
        });

        GUILayout.Space(10);

        // YooAsset配置
        _yooAssetFoldout = DrawFoldoutGroup("YooAsset配置", _yooAssetFoldout, () =>
        {
            DrawYooAssetSettings();
        });

        GUILayout.Space(10);

        // 宏定义配置
        _macroDefinesFoldout = DrawFoldoutGroup("宏定义配置", _macroDefinesFoldout, () =>
        {
            DrawMacroDefinesSettings();
        });

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 加载当前设置
    /// </summary>
    private void LoadCurrentSettings()
    {
        LoadGlobalSettings();
        LoadHybridClrSettings();
        LoadYooAssetSettings();
        LoadCurrentMacroDefines();
    }
}

}

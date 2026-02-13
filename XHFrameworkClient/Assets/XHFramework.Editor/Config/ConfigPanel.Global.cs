using UnityEngine;
using UnityEditor;

namespace XHFramework.Editor {

/// <summary>
/// 配置面板 - 全局配置部分
/// </summary>
public partial class ConfigPanel
{
    // 全局配置字段
    private string _companyName = "";
    private string _projectName = "";
    private string _version = "";

    /// <summary>
    /// 加载全局配置
    /// </summary>
    private void LoadGlobalSettings()
    {
        _companyName = PlayerSettings.companyName;
        _projectName = PlayerSettings.productName;
        _version = PlayerSettings.bundleVersion;
    }

    /// <summary>
    /// 绘制全局配置
    /// </summary>
    private void DrawGlobalSettings()
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("公司和项目信息", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("公司名:", GUILayout.Width(80));
        string newCompanyName = EditorGUILayout.TextField(_companyName);
        if (newCompanyName != _companyName)
        {
            _companyName = newCompanyName;
            PlayerSettings.companyName = _companyName;
            AssetDatabase.SaveAssets();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("项目名:", GUILayout.Width(80));
        string newProjectName = EditorGUILayout.TextField(_projectName);
        if (newProjectName != _projectName)
        {
            _projectName = newProjectName;
            PlayerSettings.productName = _projectName;
            AssetDatabase.SaveAssets();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("版本号:", GUILayout.Width(80));
        string newVersion = EditorGUILayout.TextField(_version);
        if (newVersion != _version)
        {
            _version = newVersion;
            PlayerSettings.bundleVersion = _version;
            AssetDatabase.SaveAssets();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }
}

}

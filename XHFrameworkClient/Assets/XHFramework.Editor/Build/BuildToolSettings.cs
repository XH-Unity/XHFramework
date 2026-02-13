using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace XHFramework.Editor {

/// <summary>
/// 构建工具设置（ScriptableObject 持久化存储）
/// 路径均存储为相对于项目根目录的相对路径
/// </summary>
public class BuildToolSettings : ScriptableObject
{
    private const string SettingsPath = "Assets/XHFramework.Editor/Build/BuildToolSettings.asset";

    [Header("DLL 目录")]
    public string aotDllDir ;
    public string jitDllDir ;

    [Header("输出目录")]
    public string apkOutputDir ;
    public string iOSOutputDir ;
    public string buildLogsDir ;

    [Header("DLL 列表")]
    public string aotDllsString;
    public string jitDllsString ;

    [Header("构建选项")]
    public bool enableLog = true;

    private static BuildToolSettings _instance;

    public static BuildToolSettings GetOrCreate()
    {
        if (_instance != null)
            return _instance;

        _instance = AssetDatabase.LoadAssetAtPath<BuildToolSettings>(SettingsPath);

        if (_instance == null)
        {
            _instance = CreateInstance<BuildToolSettings>();
            string dir = System.IO.Path.GetDirectoryName(SettingsPath);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            AssetDatabase.CreateAsset(_instance, SettingsPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"已创建构建工具设置文件: {SettingsPath}");
        }

        return _instance;
    }

    public void Save()
    {
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }

    // 便捷属性：获取绝对路径
    public string AotDllAbsolutePath => DataTableToolSettings.ToAbsolutePath(aotDllDir);
    public string JitDllAbsolutePath => DataTableToolSettings.ToAbsolutePath(jitDllDir);
    public string ApkOutputAbsolutePath => DataTableToolSettings.ToAbsolutePath(apkOutputDir);
    public string IOSOutputAbsolutePath => DataTableToolSettings.ToAbsolutePath(iOSOutputDir);
    public string BuildLogsAbsolutePath => DataTableToolSettings.ToAbsolutePath(buildLogsDir);

    public List<string> GetAotDLLNames()
    {
        return aotDllsString.Split(',').Where(s => !string.IsNullOrEmpty(s.Trim())).Select(s => s.Trim()).ToList();
    }

    public List<string> GetJITDLLNames()
    {
        return jitDllsString.Split(',').Where(s => !string.IsNullOrEmpty(s.Trim())).Select(s => s.Trim()).ToList();
    }
}

}

using UnityEngine;
using UnityEditor;

namespace XHFramework.Editor {

/// <summary>
/// 配置表工具路径设置（ScriptableObject 持久化存储）
/// 路径均存储为相对于项目根目录的相对路径
/// </summary>
public class DataTableToolSettings : ScriptableObject
{
    private const string SettingsPath = "Assets/XHFramework.Editor/DataTable/DataTableToolSettings.asset";

    [Header("导入脚本路径（相对于项目根目录）")]
    public string genBatPath ;

    [Header("配置表数据目录（相对于项目根目录）")]
    public string dataTableDataPath;

    [Header("数据输出目录（相对于项目根目录）")]
    public string outputDataDir ;

    [Header("代码输出目录（相对于项目根目录）")]
    public string outputCodeDir ;

    private static DataTableToolSettings _instance;

    /// <summary>
    /// 获取或创建设置实例
    /// </summary>
    public static DataTableToolSettings GetOrCreate()
    {
        if (_instance != null)
            return _instance;

        _instance = AssetDatabase.LoadAssetAtPath<DataTableToolSettings>(SettingsPath);

        if (_instance == null)
        {
            _instance = CreateInstance<DataTableToolSettings>();
            // 确保目录存在
            string dir = System.IO.Path.GetDirectoryName(SettingsPath);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            AssetDatabase.CreateAsset(_instance, SettingsPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"已创建配置表工具设置文件: {SettingsPath}");
        }

        return _instance;
    }

    /// <summary>
    /// 保存设置
    /// </summary>
    public void Save()
    {
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// 项目根目录（Assets 的上一级）
    /// </summary>
    public static string ProjectRoot => System.IO.Path.GetDirectoryName(Application.dataPath);

    /// <summary>
    /// 将相对路径转换为绝对路径
    /// </summary>
    public static string ToAbsolutePath(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return string.Empty;

        string fullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(ProjectRoot, relativePath));
        return fullPath;
    }

    /// <summary>
    /// 将绝对路径转换为相对于项目根目录的相对路径
    /// </summary>
    public static string ToRelativePath(string absolutePath)
    {
        return ToRelativePath(absolutePath, ProjectRoot);
    }

    /// <summary>
    /// 将绝对路径转换为相对于指定基准目录的相对路径
    /// </summary>
    public static string ToRelativePath(string absolutePath, string baseDir)
    {
        if (string.IsNullOrEmpty(absolutePath) || string.IsNullOrEmpty(baseDir))
            return string.Empty;

        string baseNormalized = baseDir.Replace("\\", "/");
        string absNormalized = absolutePath.Replace("\\", "/");

        if (!baseNormalized.EndsWith("/"))
            baseNormalized += "/";

        if (absNormalized.StartsWith(baseNormalized))
        {
            return absNormalized.Substring(baseNormalized.Length);
        }

        // 使用 Uri 计算相对路径
        System.Uri baseUri = new System.Uri(baseNormalized);
        System.Uri pathUri = new System.Uri(absNormalized);
        string relPath = System.Uri.UnescapeDataString(baseUri.MakeRelativeUri(pathUri).ToString());
        return relPath;
    }

    // 便捷属性：获取绝对路径
    public string GenBatAbsolutePath => ToAbsolutePath(genBatPath);
    public string DataTableDataAbsolutePath => ToAbsolutePath(dataTableDataPath);
    public string OutputDataAbsolutePath => ToAbsolutePath(outputDataDir);
    public string OutputCodeAbsolutePath => ToAbsolutePath(outputCodeDir);
}

}

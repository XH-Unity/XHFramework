using UnityEngine;
using UnityEditor;

namespace XHFramework.Editor {

/// <summary>
/// 网络模块工具路径设置（ScriptableObject 持久化存储）
/// 路径均存储为相对于项目根目录的相对路径
/// </summary>
public class NetworkToolSettings : ScriptableObject
{
    private const string SettingsPath = "Assets/XHFramework.Editor/Network/NetworkToolSettings.asset";

    [Header("Protoc 编译器路径")]
    public string protocPath ;

    [Header("TCP Proto")]
    public string tcpProtoSourcePath ;
    public string tcpProtoOutputPath ;

    [Header("UDP Proto")]
    public string udpProtoSourcePath ;
    public string udpProtoOutputPath ;

    [Header("WebSocket Proto")]
    public string webSocketProtoSourcePath ;
    public string webSocketProtoOutputPath ;

    private static NetworkToolSettings _instance;

    public static NetworkToolSettings GetOrCreate()
    {
        if (_instance != null)
            return _instance;

        _instance = AssetDatabase.LoadAssetAtPath<NetworkToolSettings>(SettingsPath);

        if (_instance == null)
        {
            _instance = CreateInstance<NetworkToolSettings>();
            string dir = System.IO.Path.GetDirectoryName(SettingsPath);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            AssetDatabase.CreateAsset(_instance, SettingsPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"已创建网络工具设置文件: {SettingsPath}");
        }

        return _instance;
    }

    public void Save()
    {
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }

    // 便捷属性：获取绝对路径
    public string ProtocAbsolutePath => DataTableToolSettings.ToAbsolutePath(protocPath);
    public string TcpProtoSourceAbsolutePath => DataTableToolSettings.ToAbsolutePath(tcpProtoSourcePath);
    public string TcpProtoOutputAbsolutePath => DataTableToolSettings.ToAbsolutePath(tcpProtoOutputPath);
    public string UdpProtoSourceAbsolutePath => DataTableToolSettings.ToAbsolutePath(udpProtoSourcePath);
    public string UdpProtoOutputAbsolutePath => DataTableToolSettings.ToAbsolutePath(udpProtoOutputPath);
    public string WebSocketProtoSourceAbsolutePath => DataTableToolSettings.ToAbsolutePath(webSocketProtoSourcePath);
    public string WebSocketProtoOutputAbsolutePath => DataTableToolSettings.ToAbsolutePath(webSocketProtoOutputPath);
}

}


namespace XHFramework.Core {

public static class ResourceConfig
{
    // 配置表：最高优先级，游戏逻辑基础数据
    public const uint DataTableAssetPriority = 100;

    // 场景：第二优先级，场景加载是核心体验
    public const uint SceneAssetPriority = 85;

    // 地图json：高优先级
    public const uint MapAssetPriority = 70;
    
    // UI界面：高优先级，玩家直接交互的界面
    public const uint UIFormAssetPriority = 60;

    // 实体：高优先级，游戏中的角色、道具等对象
    public const uint EntityAssetPriority = 55;

    // 字体：中高优先级，UI显示的基础资源
    public const uint FontAssetPriority = 50;

    // 音效：中等优先级，游戏体验的重要组成
    public const uint SoundAssetPriority = 30;

    // UI音效：中低优先级，UI反馈音效
    public const uint UISoundAssetPriority = 25;

    // 音乐：较低优先级，背景音乐可以稍后加载
    public const uint MusicAssetPriority = 20;
    
   
    
    public static string GetUISoundAsset(string assetName) => $"Assets/XHFramework.Game/PackageAssets/Audio/UISound/{assetName}.wav";
    public static string GetMusicAsset(string assetName) => $"Assets/XHFramework.Game/PackageAssets/Audio/Music/{assetName}.mp3";
    public static string GetSoundAsset(string assetName) => $"Assets/XHFramework.Game/PackageAssets/Audio/Sound/{assetName}.mp3";
    public static string GetEntityAsset(string assetName) => $"Assets/XHFramework.Game/PackageAssets/Prefabs/Entity/{assetName}.prefab";
    public static string GetUIFormAsset(string assetName) => $"Assets/XHFramework.Game/PackageAssets/Prefabs/UI/{assetName}.prefab";
    public static string GetDataTableAsset(string assetName) => $"Assets/XHFramework.Game/PackageAssets/DataTable/{assetName}.bytes";
    public static string GetMapJsonAsset(string assetName) => $"Assets/XHFramework.Game/PackageAssets/Config/{assetName}.json";
    public static string GetMapAsset(string assetName) => $"Assets/XHFramework.Game/PackageAssets/Prefabs/{assetName}.prefab";
    public static string GetSceneAsset(string assetName) => $"Assets/XHFramework.Game/PackageAssets/Scenes/{assetName}.unity";
}

}

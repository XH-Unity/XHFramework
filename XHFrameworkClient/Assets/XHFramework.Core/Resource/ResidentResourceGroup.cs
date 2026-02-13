using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using YooAsset;

namespace XHFramework.Core {

/// <summary>
/// 常驻资源组
/// 资源一旦加载就常驻内存，不提供单个释放接口
/// 只能通过 ReleaseAll 一次性释放所有资源
/// </summary>
public class ResidentResourceGroup : ResourceGroupBase
{
    /// <summary>
    /// 资源路径到Asset的映射（用于快速查找）
    /// </summary>
    protected readonly Dictionary<string, UnityEngine.Object> _pathToAsset;

    /// <summary>
    /// 资源路径到GameObject的映射（用于快速查找）
    /// </summary>
    protected readonly Dictionary<string, GameObject> _pathToGameObject;
    #region 构造函数

    public ResidentResourceGroup() : base()
    {
        _pathToAsset = new Dictionary<string, UnityEngine.Object>();
        _pathToGameObject = new Dictionary<string, GameObject>();
        
    }

    #endregion

    #region 尝试获取资源

    /// <summary>
    /// 尝试获取已加载的常驻Asset
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="assetPath">资源路径</param>
    /// <param name="asset">输出的资源对象</param>
    /// <returns>是否成功获取</returns>
    public bool TryGetAsset<T>(string assetPath, out T asset) where T : UnityEngine.Object
    {
        asset = null;

        if (string.IsNullOrEmpty(assetPath))
        {
            return false;
        }

        if (_pathToAsset.TryGetValue(assetPath, out var cachedAsset))
        {
            asset = cachedAsset as T;
            return asset != null;
        }

        return false;
    }

    /// <summary>
    /// 尝试获取已加载的常驻GameObject
    /// </summary>
    /// <param name="assetPath">资源路径</param>
    /// <param name="gameObject">输出的GameObject</param>
    /// <returns>是否成功获取</returns>
    public bool TryGetGameObject(string assetPath, out GameObject gameObject)
    {
        gameObject = null;

        if (string.IsNullOrEmpty(assetPath))
        {
            return false;
        }

        return _pathToGameObject.TryGetValue(assetPath, out gameObject);
    }

    #endregion

    #region 预加载常驻资源

    /// <summary>
    /// 预加载常驻Asset
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="assetPath">资源路径</param>
    /// <param name="priority">资源优先级</param>
    /// <returns>加载的资源</returns>
    public async UniTask<T> PreloadAssetAsync<T>(string assetPath,uint priority=0) where T : UnityEngine.Object
    {
        // 先检查是否已加载
        if (HasAsset(assetPath))
        {
            return _pathToAsset[assetPath] as T;
        }

        // 加载资源
        T asset = await LoadAssetAsync<T>(assetPath,priority);

        // 加入路径映射
        if (asset != null)
        {
            _pathToAsset[assetPath] = asset;
        }

        return asset;
    }

    /// <summary>
    /// 预加载常驻GameObject
    /// </summary>
    /// <param name="assetPath">资源路径</param>
    /// <param name="position">位置</param>
    /// <param name="rotation">旋转</param>
    /// <param name="parent">父节点</param>
    /// <returns>加载的GameObject</returns>
    public async UniTask<GameObject> PreloadGameObjectAsync(string assetPath,uint priority=0, Vector3 position=default, Quaternion rotation=default, Transform parent = null)
    {
        // 先检查是否已加载
        if (HasGameObject(assetPath))
        {
            return _pathToGameObject[assetPath];
        }

        // 加载GameObject
        GameObject go = await LoadGameObjectAsync(assetPath, priority,position, rotation, parent);

        // 加入路径映射
        if (go != null)
        {
            _pathToGameObject[assetPath] = go;
        }

        return go;
    }

    /// <summary>
    /// 批量预加载常驻Asset
    /// </summary>
    /// <param name="assetPaths">资源路径列表</param>
    /// <param name="onProgress">进度回调</param>
    public async UniTask PreloadAssetsAsync(IList<string> assetPaths, System.Action<float> onProgress = null)
    {
        if (assetPaths == null || assetPaths.Count == 0)
        {
            return;
        }

        int totalCount = assetPaths.Count;
        int loadedCount = 0;

        foreach (var path in assetPaths)
        {
            // 使用 PreloadAssetAsync 而不是 LoadAssetAsync，这样会自动检查是否已加载
            await PreloadAssetAsync<UnityEngine.Object>(path);
            loadedCount++;
            onProgress?.Invoke((float)loadedCount / totalCount);
        }

        Log.Info("[ResidentResourceGroup] 批量预加载完成，共 {0} 个资源", totalCount);
    }

    #endregion

    #region 释放所有资源

    /// <summary>
    /// 释放所有常驻资源（一般只在游戏关闭时调用）
    /// </summary>
    public override void ReleaseAll()
    {
        base.ReleaseAll();

        // 清理路径映射
        _pathToAsset.Clear();
        _pathToGameObject.Clear();

        Log.Info("[ResidentResourceGroup] 已释放所有常驻资源");
    }

    #endregion
    
    #region 内部方法

    /// <summary>
    /// 检查是否持有指定Asset
    /// </summary>
    public bool IsHoldingAsset(UnityEngine.Object asset)
    {
        return asset != null && _assetToHandle.ContainsKey(asset);
    }

    /// <summary>
    /// 检查是否持有指定GameObject
    /// </summary>
    public bool IsHoldingGameObject(GameObject gameObject)
    {
        return gameObject != null && _gameObjectToHandle.ContainsKey(gameObject);
    }

    /// <summary>
    /// 通过路径检查是否已加载Asset
    /// </summary>
    public bool HasAsset(string assetPath)
    {
        return !string.IsNullOrEmpty(assetPath) && _pathToAsset.ContainsKey(assetPath);
    }

    /// <summary>
    /// 通过路径检查是否已加载GameObject
    /// </summary>
    public bool HasGameObject(string assetPath)
    {
        return !string.IsNullOrEmpty(assetPath) && _pathToGameObject.ContainsKey(assetPath);
    }

    #endregion
}

}

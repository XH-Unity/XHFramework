using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using YooAsset;

namespace XHFramework.Core {

/// <summary>
/// 资源组抽象基类
/// 提供资源加载的基础功能，子类实现不同的释放策略
/// </summary>
public abstract class ResourceGroupBase
{
    #region 字段

    /// <summary>
    /// Asset到Handle的映射
    /// </summary>
    protected readonly Dictionary<UnityEngine.Object, AssetHandle> _assetToHandle;

    /// <summary>
    /// GameObject实例到Handle的映射
    /// </summary>
    protected readonly Dictionary<GameObject, AssetHandle> _gameObjectToHandle;
    #endregion

  
    #region 构造函数

    protected ResourceGroupBase()
    {
        _assetToHandle = new Dictionary<UnityEngine.Object, AssetHandle>();
        _gameObjectToHandle = new Dictionary<GameObject, AssetHandle>();
    }

    #endregion

    #region 异步加载

    /// <summary>
    /// 异步加载资源
    /// </summary>
    public async UniTask<T> LoadAssetAsync<T>(string assetPath,uint priority) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(assetPath))
        {
            Log.Error("[ResourceGroup] 资源路径为空");
            return null;
        }

        // 调用 YooAsset 加载
        AssetHandle handle = YooAssets.LoadAssetAsync<T>(assetPath,priority);
        await handle.ToUniTask();

        if (handle.Status != EOperationStatus.Succeed)
        {
            Log.Error("[ResourceGroup] 加载资源失败: {0}, 错误: {1}", assetPath, handle.LastError);
            return null;
        }

        T asset = handle.AssetObject as T;
        if (asset != null)
        {
            _assetToHandle[asset] = handle;
        }

        return asset;
    }

    /// <summary>
    /// 异步加载并实例化GameObject
    /// </summary>
    public async UniTask<GameObject> LoadGameObjectAsync(string assetPath,uint priority=0, Vector3 position=default, Quaternion rotation=default, Transform parent = null)
    {
        if (string.IsNullOrEmpty(assetPath))
        {
            Log.Error("[ResourceGroup] 资源路径为空");
            return null;
        }

        // 调用 YooAsset 加载
        AssetHandle handle = YooAssets.LoadAssetAsync<GameObject>(assetPath,priority);
        await handle.ToUniTask();

        if (handle.Status != EOperationStatus.Succeed)
        {
            Log.Error("[ResourceGroup] 加载GameObject失败: {0}, 错误: {1}", assetPath, handle.LastError);
            return null;
        }

        // 实例化
        GameObject prefab = handle.AssetObject as GameObject;
        if (prefab == null)
        {
            Log.Error("[ResourceGroup] 资源类型不是GameObject: {0}", assetPath);
            handle.Release();
            return null;
        }

        GameObject instance = handle.InstantiateSync(position, rotation, parent);
        _gameObjectToHandle[instance] = handle;

        return instance;
    }

    #endregion

    #region 立即释放所有资源

    /// <summary>
    /// 立即释放所有资源（无延迟）
    /// </summary>
    public virtual void ReleaseAll()
    {
        // 释放所有Asset
        foreach (var kvp in _assetToHandle)
        {
            kvp.Value?.Release();
        }
        _assetToHandle.Clear();

        // 销毁并释放所有GameObject
        foreach (var kvp in _gameObjectToHandle)
        {
            if (kvp.Key != null)
            {
                UnityEngine.Object.Destroy(kvp.Key);
            }
            kvp.Value?.Release();
        }
        _gameObjectToHandle.Clear();

        Log.Info("[ResourceGroup] 已释放所有资源");
    }

    #endregion


}

}

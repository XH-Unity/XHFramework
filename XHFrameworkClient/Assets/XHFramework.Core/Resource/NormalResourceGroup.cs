using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using YooAsset;

namespace XHFramework.Core {

/// <summary>
/// 正常资源组
/// 支持延迟释放机制，资源在指定延迟时间后才真正释放
/// </summary>
public class NormalResourceGroup : ResourceGroupBase
{
    #region 字段

    /// <summary>
    /// 默认延迟释放时间（秒）
    /// </summary>
    private readonly float _defaultDelayTime;

    /// <summary>
    /// 延迟释放任务列表（用于取消延迟释放）
    /// </summary>
    private readonly Dictionary<UnityEngine.Object, System.Threading.CancellationTokenSource> _assetDelayTasks;
    private readonly Dictionary<GameObject, System.Threading.CancellationTokenSource> _gameObjectDelayTasks;

    #endregion

    #region 构造函数

    /// <summary>
    /// 创建正常资源组
    /// </summary>
    /// <param name="defaultDelayTime">默认延迟释放时间（秒）</param>
    public NormalResourceGroup(float defaultDelayTime = 30f) : base()
    {
        _defaultDelayTime = defaultDelayTime;
        _assetDelayTasks = new Dictionary<UnityEngine.Object, System.Threading.CancellationTokenSource>();
        _gameObjectDelayTasks = new Dictionary<GameObject, System.Threading.CancellationTokenSource>();
    }

    #endregion

    #region 延迟释放

    /// <summary>
    /// 释放Asset资源（延迟释放）
    /// </summary>
    /// <param name="asset">要释放的资源</param>
    /// <param name="delayTime">延迟时间（秒），-1 表示使用默认延迟时间</param>
    public void Release(UnityEngine.Object asset, float delayTime = -1f)
    {
        if (asset == null) return;

        if (!_assetToHandle.TryGetValue(asset, out AssetHandle handle))
        {
            return;
        }

        float actualDelay = delayTime < 0 ? _defaultDelayTime : delayTime;

        // 如果延迟时间为0，立即释放
        if (actualDelay <= 0)
        {
            ReleaseAssetImmediate(asset, handle);
            return;
        }

        // 取消之前的延迟任务（如果有）
        CancelAssetDelayTask(asset);

        // 创建新的延迟释放任务
        var cts = new System.Threading.CancellationTokenSource();
        _assetDelayTasks[asset] = cts;

        DelayedReleaseAssetAsync(asset, handle, actualDelay, cts.Token).Forget();
    }

    /// <summary>
    /// 释放GameObject资源（延迟释放）
    /// </summary>
    /// <param name="gameObject">要释放的GameObject</param>
    /// <param name="destroyGameObject">是否销毁GameObject</param>
    /// <param name="delayTime">延迟时间（秒），-1 表示使用默认延迟时间</param>
    public void ReleaseGameObject(GameObject gameObject, bool destroyGameObject = true, float delayTime = -1f)
    {
        if (gameObject == null) return;

        if (!_gameObjectToHandle.TryGetValue(gameObject, out AssetHandle handle))
        {
            return;
        }

        float actualDelay = delayTime < 0 ? _defaultDelayTime : delayTime;

        // 如果延迟时间为0，立即释放
        if (actualDelay <= 0)
        {
            ReleaseGameObjectImmediate(gameObject, handle, destroyGameObject);
            return;
        }

        // 取消之前的延迟任务（如果有）
        CancelGameObjectDelayTask(gameObject);

        // 创建新的延迟释放任务
        var cts = new System.Threading.CancellationTokenSource();
        _gameObjectDelayTasks[gameObject] = cts;

        DelayedReleaseGameObjectAsync(gameObject, handle, destroyGameObject, actualDelay, cts.Token).Forget();
    }

    #endregion

    #region 立即释放

    /// <summary>
    /// 立即释放Asset（内部方法）
    /// </summary>
    private void ReleaseAssetImmediate(UnityEngine.Object asset, AssetHandle handle)
    {
        // 从路径映射中移除
        // 释放Handle
        handle?.Release();
        _assetToHandle.Remove(asset);
        _assetDelayTasks.Remove(asset);
    }

    /// <summary>
    /// 立即释放GameObject（内部方法）
    /// </summary>
    private void ReleaseGameObjectImmediate(GameObject gameObject, AssetHandle handle, bool destroyGameObject)
    {
        // 销毁GameObject
        if (destroyGameObject && gameObject != null)
        {
            UnityEngine.Object.Destroy(gameObject);
        }

        // 释放Handle
        handle?.Release();
        _gameObjectToHandle.Remove(gameObject);
        _gameObjectDelayTasks.Remove(gameObject);
    }

    #endregion

    #region 延迟释放异步方法

    /// <summary>
    /// 延迟释放Asset的异步任务
    /// </summary>
    private async UniTaskVoid DelayedReleaseAssetAsync(UnityEngine.Object asset, AssetHandle handle, float delay, System.Threading.CancellationToken token)
    {
        try
        {
            await UniTask.Delay((int)(delay * 1000), cancellationToken: token);

            // 检查资源是否还存在于字典中（可能已被重新加载或取消释放）
            if (_assetToHandle.ContainsKey(asset))
            {
                ReleaseAssetImmediate(asset, handle);
                Log.Info("[NormalResourceGroup] 延迟释放Asset完成");
            }
        }
        catch (System.OperationCanceledException)
        {
            // 任务被取消，不做任何操作
        }
    }

    /// <summary>
    /// 延迟释放GameObject的异步任务
    /// </summary>
    private async UniTaskVoid DelayedReleaseGameObjectAsync(GameObject gameObject, AssetHandle handle, bool destroyGameObject, float delay, System.Threading.CancellationToken token)
    {
        try
        {
            await UniTask.Delay((int)(delay * 1000), cancellationToken: token);

            // 检查GameObject是否还存在于字典中
            if (_gameObjectToHandle.ContainsKey(gameObject))
            {
                ReleaseGameObjectImmediate(gameObject, handle, destroyGameObject);
                Log.Info("[NormalResourceGroup] 延迟释放GameObject完成");
            }
        }
        catch (System.OperationCanceledException)
        {
            // 任务被取消，不做任何操作
        }
    }

    #endregion

    #region 取消延迟任务

    /// <summary>
    /// 取消Asset的延迟释放任务
    /// </summary>
    private void CancelAssetDelayTask(UnityEngine.Object asset)
    {
        if (_assetDelayTasks.TryGetValue(asset, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
            _assetDelayTasks.Remove(asset);
        }
    }

    /// <summary>
    /// 取消GameObject的延迟释放任务
    /// </summary>
    private void CancelGameObjectDelayTask(GameObject gameObject)
    {
        if (_gameObjectDelayTasks.TryGetValue(gameObject, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
            _gameObjectDelayTasks.Remove(gameObject);
        }
    }

    #endregion

    #region 重写释放所有

    /// <summary>
    /// 立即释放所有资源（无延迟，取消所有延迟任务）
    /// </summary>
    public override void ReleaseAll()
    {
        // 取消所有延迟任务
        foreach (var cts in _assetDelayTasks.Values)
        {
            cts.Cancel();
            cts.Dispose();
        }
        _assetDelayTasks.Clear();

        foreach (var cts in _gameObjectDelayTasks.Values)
        {
            cts.Cancel();
            cts.Dispose();
        }
        _gameObjectDelayTasks.Clear();

        // 调用基类的释放方法
        base.ReleaseAll();
    }

    #endregion
}

}

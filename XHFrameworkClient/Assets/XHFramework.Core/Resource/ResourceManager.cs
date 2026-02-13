using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using YooAsset;

namespace XHFramework.Core {

/// <summary>
/// 资源管理器
/// 整合正常资源组和常驻资源组，提供统一的资源加载/释放接口
/// 加载时优先检查常驻资源组，释放时自动跳过常驻资源
/// </summary>
public partial class ResourceManager : ManagerBase
{
    public override int Priority => 110;

    #region 字段

    /// <summary>
    /// 正常资源组
    /// </summary>
    private NormalResourceGroup _normalGroup;

    /// <summary>
    /// 全局常驻资源组
    /// </summary>
    private ResidentResourceGroup _globalResidentGroup;

    /// <summary>
    /// 场景常驻资源组
    /// </summary>
    private ResidentResourceGroup _sceneResidentGroup;

    /// <summary>
    /// 临时资源组（加载后立即释放句柄）
    /// </summary>
    private TransientResourceGroup _transientGroup;

    /// <summary>
    /// 场景管理组
    /// </summary>
    private SceneGroup _sceneGroup;

    /// <summary>
    /// 资源配置
    /// </summary>
    private ResourceMemoryConfig _memoryConfig;

    /// <summary>
    /// 当前内存信息
    /// </summary>
    private MemoryInfo _memoryInfo;

    /// <summary>
    /// 上次内存检查时间
    /// </summary>
    private float _lastMemoryCheckTime;

    #endregion
    
    #region 生命周期
    public override void Init()
    {
        _memoryConfig = ResourceMemoryConfig.GetPlatformDefault();
        _normalGroup = new NormalResourceGroup(_memoryConfig.ReleaseDelayTime);
        _globalResidentGroup = new ResidentResourceGroup();
        _sceneResidentGroup = new ResidentResourceGroup();
        _transientGroup = new TransientResourceGroup();
        _sceneGroup = new SceneGroup();
        _memoryInfo = new MemoryInfo();
        
        // 初始化内存信息
        UpdateMemoryInfo();

        Log.Info("[ResourceManager] 资源管理器初始化完成, 平台配置: 默认延迟释放时间={0}秒, 内存警告阈值={1}MB",
            _memoryConfig.ReleaseDelayTime, _memoryConfig.MemoryWarningThresholdMB);
    }

    public override void Update(float elapseSeconds, float realElapseSeconds)
    {
        float currentTime = Time.realtimeSinceStartup;

        // 内存检查
        if (currentTime - _lastMemoryCheckTime >= _memoryConfig.MemoryCheckInterval)
        {
            _lastMemoryCheckTime = currentTime;
            UpdateMemoryInfo();

            // 根据内存压力自动释放
            if (_memoryConfig.EnableMemoryPressureRelease)
            {
                HandleMemoryPressure();
            }
        }
    }

    public override void Shutdown()
    {
        // 释放所有资源
        _normalGroup?.ReleaseAll();
        _sceneResidentGroup?.ReleaseAll();
        _sceneGroup?.ReleaseAll();
        _globalResidentGroup?.ReleaseAll();

        _normalGroup = null;
        _sceneResidentGroup = null;
        _globalResidentGroup = null;
        _transientGroup = null;
        _sceneGroup = null;
        _memoryConfig = null;
        _memoryInfo = null;
        Log.Info("[ResourceManager] 资源管理器已关闭");
    }

    #endregion

    #region 加载资源

    /// <summary>
    /// 异步加载资源并立即释放句柄
    /// 适用于配置文件、JSON、ScriptableObject等读取数据后不需要保持引用的资源
    /// 注意：返回的资源对象在YooAsset内部引用计数归零后可能被卸载，请确保及时复制所需数据
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="assetPath">资源路径</param>
    /// <param name="priority">加载优先级</param>
    /// <returns>加载的资源对象</returns>
    public async UniTask<T> LoadAssetAndReleaseAsync<T>(string assetPath, uint priority = 0) where T : UnityEngine.Object
    {
        return await _transientGroup.LoadAssetAsync<T>(assetPath, priority);
    }

    /// <summary>
    /// 通过标签批量加载资源并立即释放句柄
    /// 适用于批量加载配置文件等场景
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="tag">资源标签</param>
    /// <param name="priority">加载优先级</param>
    /// <returns>资源信息和资源对象的字典</returns>
    public async UniTask<Dictionary<string, T>> LoadAssetsByTagAndReleaseAsync<T>(string tag, uint priority = 0) where T : UnityEngine.Object
    {
        return await _transientGroup.LoadAssetsByTagAsync<T>(tag, priority);
    }

    /// <summary>
    /// 异步加载Asset资源
    /// 优先从常驻资源组获取，如果没有则从正常资源组加载
    /// </summary>
    public async UniTask<T> LoadAssetAsync<T>(string assetPath,uint priority=0) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(assetPath))
        {
            Log.Error("[ResourceManager] 资源路径为空");
            return null;
        }

        // 1. 优先从全局常驻资源组获取
        if (_globalResidentGroup.TryGetAsset<T>(assetPath, out var globalResidentAsset))
        {
            return globalResidentAsset;
        }
        
        // 2. 优先从场景常驻资源组获取
        if (_sceneResidentGroup.TryGetAsset<T>(assetPath, out var sceneResidentAsset))
        {
            return sceneResidentAsset;
        }

        // 2. 从正常资源组加载
        return await _normalGroup.LoadAssetAsync<T>(assetPath,priority);
    }

    /// <summary>
    /// 异步加载并实例化GameObject
    /// 优先从常驻资源组获取，如果没有则从正常资源组加载
    /// </summary>
    public async UniTask<GameObject> LoadGameObjectAsync(string assetPath,uint priority=0, Vector3 position=default, Quaternion rotation=default, Transform parent = null)
    {
        if (string.IsNullOrEmpty(assetPath))
        {
            Log.Error("[ResourceManager] 资源路径为空");
            return null;
        }

        // 1. 优先从全局常驻资源组获取（常驻资源组的GameObject是预实例化的）
        if (_globalResidentGroup.TryGetGameObject(assetPath, out var globalResidentGo))
        {
            return globalResidentGo;
        }
        
        // 1. 优先从场景常驻资源组获取（常驻资源组的GameObject是预实例化的）
        if (_sceneResidentGroup.TryGetGameObject(assetPath, out var sceneResidentGo))
        {
            return sceneResidentGo;
        }

        // 2. 从正常资源组加载
        return await _normalGroup.LoadGameObjectAsync(assetPath,priority, position, rotation, parent);
    }
    #endregion

    #region 释放资源

    /// <summary>
    /// 释放Asset资源
    /// 如果是常驻资源则跳过释放
    /// </summary>
    /// <param name="asset">要释放的资源</param>
    /// <param name="delayTime">延迟时间（秒），-1 表示使用默认延迟时间</param>
    public void Release(UnityEngine.Object asset, float delayTime = -1f)
    {
        if (asset == null) return;

        // 检查是否是常驻资源，常驻资源不释放
        if (_globalResidentGroup.IsHoldingAsset(asset))
        {
            return;
        }
        
        // 检查是否是常驻资源，常驻资源不释放
        if (_sceneResidentGroup.IsHoldingAsset(asset))
        {
            return;
        }

        // 从正常资源组释放
        _normalGroup.Release(asset, delayTime);
    }

    /// <summary>
    /// 释放GameObject
    /// 如果是常驻资源则跳过释放
    /// </summary>
    /// <param name="gameObject">要释放的GameObject</param>
    /// <param name="destroyGameObject">是否销毁GameObject</param>
    /// <param name="delayTime">延迟时间（秒），-1 表示使用默认延迟时间</param>
    public void ReleaseGameObject(GameObject gameObject, bool destroyGameObject = true, float delayTime = -1f)
    {
        if (gameObject == null) return;

        // 检查是否是常驻资源，常驻资源不释放
        if (_globalResidentGroup.IsHoldingGameObject(gameObject))
        {
            return;
        }
        // 检查是否是常驻资源，常驻资源不释放
        if (_sceneResidentGroup.IsHoldingGameObject(gameObject))
        {
            return;
        }
        // 从正常资源组释放
        _normalGroup.ReleaseGameObject(gameObject, destroyGameObject, delayTime);
    }
   
    #endregion

    #region 预加载常驻资源

    /// <summary>
    /// 预加载常驻Asset资源
    /// </summary>
    public async UniTask<T> PreloadGlobalResidentAssetAsync<T>(string assetPath,uint priority=0) where T : UnityEngine.Object
    {
        return await _globalResidentGroup.PreloadAssetAsync<T>(assetPath);
    }

    /// <summary>
    /// 预加载常驻GameObject
    /// </summary>
    public async UniTask<GameObject> PreloadGlobalResidentGameObjectAsync(string assetPath,uint priority=0, Vector3 position=default, Quaternion rotation=default, Transform parent = null)
    {
        return await _globalResidentGroup.PreloadGameObjectAsync(assetPath,priority, position, rotation, parent);
    }

    /// <summary>
    /// 预加载场景常驻Asset资源
    /// </summary>
    public async UniTask<T> PreloadSceneResidentAssetAsync<T>(string assetPath,uint priority=0) where T : UnityEngine.Object
    {
        return await _sceneResidentGroup.PreloadAssetAsync<T>(assetPath);
    }

    /// <summary>
    /// 预加载场景常驻GameObject
    /// </summary>
    public async UniTask<GameObject> PreloadSceneResidentGameObjectAsync(string assetPath,uint priority=0, Vector3 position=default, Quaternion rotation=default, Transform parent = null)
    {
        return await _sceneResidentGroup.PreloadGameObjectAsync(assetPath, priority,position, rotation, parent);
    }
    #endregion

    #region 场景管理
    /// <summary>
    /// 异步加载场景
    /// </summary>
    /// <param name="scenePath">场景资源路径</param>
    /// <param name="loadMode">加载模式</param>
    /// <param name="onProgress">加载进度回调</param>
    /// <returns>是否加载成功</returns>
    public async UniTask<bool> LoadSceneAsync(string scenePath, LoadSceneMode loadMode= LoadSceneMode.Additive,uint priority=100, Action<float> onProgress = null)
    {
        if (_sceneGroup.IsSceneLoaded(scenePath))
        {
            return false;
        }
        return await _sceneGroup.LoadSceneAsync(scenePath, loadMode, priority,onProgress);
    }

    /// <summary>
    /// 异步卸载叠加场景
    /// </summary>
    /// <param name="scenePath">场景资源路径</param>
    /// <returns>是否卸载成功</returns>
    public async UniTask<bool> UnloadSceneAsync(string scenePath)
    {
        return await _sceneGroup.UnloadSceneAsync(scenePath);
    }

    /// <summary>
    /// 卸载所有叠加场景
    /// </summary>
    public async UniTask UnloadAllAdditiveScenesAsync()
    {
        await _sceneGroup.UnloadAllAdditiveScenes();
    }
    /// <summary>
    /// 释放所有资源（切换场景时调用）
    /// </summary>
    public async UniTask UnloadSceneAssetsAsync()
    {
        // 1. 先清理Unity原生未使用资源
        await Resources.UnloadUnusedAssets();
        _normalGroup.ReleaseAll();
        _sceneResidentGroup.ReleaseAll();
        // 2. 再清理YooAsset未使用资源
        var package = YooAssets.GetPackage("DefaultPackage");
        if (package != null)
        {
            var operation = package.UnloadUnusedAssetsAsync();
            await operation.ToUniTask();
        }
        // 3. 最后GC
        GC.Collect();
        Log.Info("[ResourceManager] 已卸载未使用资源");
    }

    #endregion

}
}

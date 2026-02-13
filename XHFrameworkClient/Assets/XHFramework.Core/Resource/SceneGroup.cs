using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using YooAsset;

namespace XHFramework.Core {

/// <summary>
/// 场景管理组
/// 负责场景的加载、卸载和切换
/// </summary>
public class SceneGroup
{
    /// <summary>
    /// 当前加载的场景Handle映射（场景名 -> Handle）
    /// </summary>
    private readonly Dictionary<string, SceneHandle> _loadedScenes;

    /// <summary>
    /// 当前主场景名称
    /// </summary>
    private string _currentMainScene;
    
    public SceneGroup()
    {
        _loadedScenes = new Dictionary<string, SceneHandle>();
        _currentMainScene = null;
    }

    /// <summary>
    /// 异步加载场景
    /// </summary>
    /// <param name="scenePath">场景资源路径</param>
    /// <param name="loadMode">加载模式</param>
    /// <param name="onProgress">加载进度回调</param>
    /// <returns>是否加载成功</returns>
    public async UniTask<bool> LoadSceneAsync(string scenePath, LoadSceneMode loadMode, uint priority , Action<float> onProgress = null)
    {
        if (string.IsNullOrEmpty(scenePath))
        {
            Log.Error("[SceneGroup] 场景路径为空");
            return false;
        }
        // 如果是Single模式，先记录要卸载的旧场景
        string oldMainScene = null;
        if (loadMode == LoadSceneMode.Single && !string.IsNullOrEmpty(_currentMainScene))
        {
            oldMainScene = _currentMainScene;
        }

        // 使用YooAsset加载场景
        SceneHandle handle = YooAssets.LoadSceneAsync(scenePath, loadMode, LocalPhysicsMode.None, false, priority);

        // 等待加载完成，同时报告进度
        while (!handle.IsDone)
        {
            float progress = handle.Progress;
            onProgress?.Invoke(progress);
            await UniTask.Yield();
        }

        // 加载完成时报告100%进度
        onProgress?.Invoke(1f);

        // 检查加载结果
        if (handle.Status != EOperationStatus.Succeed)
        {
            Log.Error("[SceneGroup] 加载场景失败: {0}, 错误: {1}", scenePath, handle.LastError);
            return false;
        }

        // Single模式下，清理旧场景的Handle
        if (loadMode == LoadSceneMode.Single)
        {
            // 清理所有旧的场景Handle（YooAsset在Single模式下会自动卸载旧场景）
            _loadedScenes.Clear();
            _currentMainScene = scenePath;
        }

        // 记录新场景
        _loadedScenes[scenePath] = handle;

        // 如果是Single模式，更新主场景
        if (loadMode == LoadSceneMode.Single)
        {
            _currentMainScene = scenePath;
        }

        Log.Info("[SceneGroup] 场景加载完成: {0}, 模式: {1}", scenePath, loadMode);
        return true;
    }


    #region 卸载场景

    /// <summary>
    /// 异步卸载叠加场景
    /// </summary>
    /// <param name="scenePath">场景资源路径</param>
    /// <returns>是否卸载成功</returns>
    public async UniTask<bool> UnloadSceneAsync(string scenePath)
    {
        if (string.IsNullOrEmpty(scenePath))
        {
            Log.Error("[SceneGroup] 场景路径为空");
            return false;
        }

        // 不能卸载主场景
        if (scenePath == _currentMainScene)
        {
            Log.Error("[SceneGroup] 不能直接卸载主场景，请使用 LoadSceneAsync 切换到新场景");
            return false;
        }

        // 检查场景是否已加载
        if (!_loadedScenes.TryGetValue(scenePath, out SceneHandle handle))
        {
            Log.Warn("[SceneGroup] 场景未加载，无需卸载: {0}", scenePath);
            return true;
        }

        // 卸载场景
        var unloadOperation = handle.UnloadAsync();
        await unloadOperation.ToUniTask();

        // 移除记录
        _loadedScenes.Remove(scenePath);
        Log.Info("[SceneGroup] 场景卸载完成: {0}", scenePath);
        return true;
    }

    /// <summary>
    /// 卸载所有叠加场景（保留主场景）
    /// </summary>
    public async UniTask UnloadAllAdditiveScenes()
    {
        List<string> scenesToUnload = new List<string>();

        foreach (var scenePath in _loadedScenes.Keys)
        {
            if (scenePath != _currentMainScene)
            {
                scenesToUnload.Add(scenePath);
            }
        }

        foreach (var scenePath in scenesToUnload)
        {
            await UnloadSceneAsync(scenePath);
        }

        Log.Info("[SceneGroup] 已卸载所有叠加场景，共 {0} 个", scenesToUnload.Count);
    }

    #endregion

  

    /// <summary>
    /// 检查场景是否已加载
    /// </summary>
    public bool IsSceneLoaded(string scenePath)
    {
        return !string.IsNullOrEmpty(scenePath) && _loadedScenes.ContainsKey(scenePath);
    }
    
    #region 释放

    /// <summary>
    /// 释放所有场景资源（一般只在游戏关闭时调用）
    /// </summary>
    public void ReleaseAll()
    {
        // 销毁并释放所有GameObject
        foreach (var kvp in _loadedScenes)
        {
            kvp.Value?.Release();
        }
        _loadedScenes.Clear();
        _currentMainScene = null;
    }

    #endregion
}

}

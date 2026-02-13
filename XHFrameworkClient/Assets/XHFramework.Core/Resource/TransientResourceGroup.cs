using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace XHFramework.Core
{

/// <summary>
/// 临时资源组
/// 加载资源后立即释放句柄，适用于配置文件、JSON、DataTable等读取数据后不需要保持引用的资源
/// 注意：返回的资源对象在YooAsset内部引用计数归零后可能被卸载，请确保及时复制所需数据
/// </summary>
public class TransientResourceGroup
{
    /// <summary>
    /// 异步加载资源并立即释放句柄
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="assetPath">资源路径</param>
    /// <param name="priority">加载优先级</param>
    /// <returns>加载的资源对象</returns>
    public async UniTask<T> LoadAssetAsync<T>(string assetPath, uint priority = 0) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(assetPath))
        {
            Log.Error("[TransientResourceGroup] 资源路径为空");
            return null;
        }

        try
        {
            var handle = YooAssets.LoadAssetAsync<T>(assetPath, priority);
            await handle.ToUniTask();

            if (handle.Status == EOperationStatus.Succeed)
            {
                T asset = handle.AssetObject as T;
                handle.Release();
                return asset;
            }
            else
            {
                Log.Error("[TransientResourceGroup] 加载资源失败: {0}, 状态: {1}", assetPath, handle.Status);
                handle.Release();
                return null;
            }
        }
        catch (Exception e)
        {
            Log.Error("[TransientResourceGroup] 加载资源异常: {0}, {1}", assetPath, e.Message);
            return null;
        }
    }

    /// <summary>
    /// 通过标签批量加载资源并立即释放句柄
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="tag">资源标签</param>
    /// <param name="priority">加载优先级</param>
    /// <returns>资源路径和资源对象的字典</returns>
    public async UniTask<Dictionary<string, T>> LoadAssetsByTagAsync<T>(string tag, uint priority = 0) where T : UnityEngine.Object
    {
        var result = new Dictionary<string, T>();

        if (string.IsNullOrEmpty(tag))
        {
            Log.Error("[TransientResourceGroup] 标签为空");
            return result;
        }

        var assetInfos = YooAssets.GetAssetInfos(tag);
        if (assetInfos == null || assetInfos.Length == 0)
        {
            Log.Warn("[TransientResourceGroup] 未找到标签为 {0} 的资源", tag);
            return result;
        }

        Log.Info("[TransientResourceGroup] 开始加载标签 {0} 下的 {1} 个资源", tag, assetInfos.Length);

        foreach (var assetInfo in assetInfos)
        {
            try
            {
                var handle = YooAssets.LoadAssetAsync<T>(assetInfo.AssetPath, priority);
                await handle.ToUniTask();

                if (handle.Status == EOperationStatus.Succeed)
                {
                    T asset = handle.AssetObject as T;
                    if (asset != null)
                    {
                        result[assetInfo.AssetPath] = asset;
                    }
                }
                else
                {
                    Log.Error("[TransientResourceGroup] 加载资源失败: {0}", assetInfo.AssetPath);
                }

                handle.Release();
            }
            catch (Exception e)
            {
                Log.Error("[TransientResourceGroup] 加载资源异常: {0}, {1}", assetInfo.AssetPath, e.Message);
            }
        }

        Log.Info("[TransientResourceGroup] 标签 {0} 资源加载完成，成功 {1}/{2}", tag, result.Count, assetInfos.Length);
        return result;
    }
}

}

using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using YooAsset;

namespace XHFramework.Core {

/// <summary>
/// 资源管理器 - 内存管理模块
/// </summary>
public partial class ResourceManager
{
     #region 内存压力监控

    /// <summary>
    /// 更新内存信息
    /// </summary>
    private void UpdateMemoryInfo()
    {
        _memoryInfo.TotalMemoryMB = SystemInfo.systemMemorySize;
        _memoryInfo.MonoHeapMB = UnityEngine.Profiling.Profiler.GetMonoHeapSizeLong() / (1024f * 1024f);
        _memoryInfo.MonoUsedMB = UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong() / (1024f * 1024f);
        _memoryInfo.GraphicsMemoryMB = UnityEngine.Profiling.Profiler.GetAllocatedMemoryForGraphicsDriver() / (1024f * 1024f);

        // 估算总使用内存
        _memoryInfo.UsedMemoryMB = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);

        // 计算压力级别
        if (_memoryInfo.UsedMemoryMB >= _memoryConfig.MemoryEmergencyThresholdMB)
        {
            _memoryInfo.PressureLevel = MemoryPressureLevel.Emergency;
        }
        else if (_memoryInfo.UsedMemoryMB >= _memoryConfig.MemoryCriticalThresholdMB)
        {
            _memoryInfo.PressureLevel = MemoryPressureLevel.Critical;
        }
        else if (_memoryInfo.UsedMemoryMB >= _memoryConfig.MemoryWarningThresholdMB)
        {
            _memoryInfo.PressureLevel = MemoryPressureLevel.Warning;
        }
        else
        {
            _memoryInfo.PressureLevel = MemoryPressureLevel.Normal;
        }
    }

    /// <summary>
    /// 处理内存压力
    /// </summary>
    private void HandleMemoryPressure()
    {
        switch (_memoryInfo.PressureLevel)
        {
            case MemoryPressureLevel.Warning:
                // 警告级别：卸载未使用资源
                UnloadUnusedAssets();
                break;

            case MemoryPressureLevel.Critical:
                // 严重级别：释放所有正常资源，卸载未使用资源
                _normalGroup.ReleaseAll();
                UnloadUnusedAssets();
                Log.Warn("[ResourceManager] 内存压力严重({0:F1}MB)，已清理正常资源", _memoryInfo.UsedMemoryMB);
                break;

            case MemoryPressureLevel.Emergency:
                // 紧急级别：释放所有正常资源，强制GC
                _normalGroup.ReleaseAll();
                UnloadUnusedAssets();
                ForceGC();
                Log.Error("[ResourceManager] 内存压力紧急({0:F1}MB)，已强制清理资源", _memoryInfo.UsedMemoryMB);
                break;
        }
    }
    #endregion
    
    #region 内存管理

    /// <summary>
    /// 同步卸载未使用资源
    /// </summary>
    private void UnloadUnusedAssets()
    {
        Resources.UnloadUnusedAssets();

        var package = YooAssets.GetPackage("DefaultPackage");
        package?.UnloadUnusedAssetsAsync();

        GC.Collect();

        Log.Info("[ResourceManager] 已卸载未使用资源");
    }

    /// <summary>
    /// 强制GC
    /// </summary>
    private void ForceGC()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    #endregion


}

}

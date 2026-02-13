using System;
using UnityEngine;

namespace XHFramework.Core {

/// <summary>
/// 资源管理配置
/// 根据平台和设备性能自动调整参数
/// </summary>
[Serializable]
public class ResourceMemoryConfig
{
    #region 内存配置

    /// <summary>
    /// 内存压力阈值 - 警告级别（MB）
    /// </summary>
    public float MemoryWarningThresholdMB { get; set; } = 512f;

    /// <summary>
    /// 内存压力阈值 - 严重级别（MB）
    /// </summary>
    public float MemoryCriticalThresholdMB { get; set; } = 768f;

    /// <summary>
    /// 内存压力阈值 - 紧急级别（MB）
    /// </summary>
    public float MemoryEmergencyThresholdMB { get; set; } = 1024f;

    /// <summary>
    /// 内存检查间隔（秒）
    /// </summary>
    public float MemoryCheckInterval { get; set; } = 5f;

    /// <summary>
    /// 启用内存压力自动释放
    /// </summary>
    public bool EnableMemoryPressureRelease { get; set; } = true;

    #endregion

    #region 释放配置

    /// <summary>
    /// 延迟释放时间（秒）
    /// </summary>
    public float ReleaseDelayTime { get; set; } = 30f;

    #endregion

    #region 平台预设

    /// <summary>
    /// 获取当前平台的默认配置
    /// </summary>
    public static ResourceMemoryConfig GetPlatformDefault()
    {
        var config = new ResourceMemoryConfig();

#if UNITY_ANDROID || UNITY_IOS
        // 移动平台配置
        config.MemoryWarningThresholdMB = 256f;
        config.MemoryCriticalThresholdMB = 384f;
        config.MemoryEmergencyThresholdMB = 512f;
        config.ReleaseDelayTime = 20f;
#elif UNITY_WEBGL
        // WebGL配置（内存受限）
        config.MemoryWarningThresholdMB = 128f;
        config.MemoryCriticalThresholdMB = 192f;
        config.MemoryEmergencyThresholdMB = 256f;
        config.ReleaseDelayTime = 10f;
#elif UNITY_STANDALONE
        // PC/Mac配置
        config.MemoryWarningThresholdMB = 1024f;
        config.MemoryCriticalThresholdMB = 1536f;
        config.MemoryEmergencyThresholdMB = 2048f;
        config.ReleaseDelayTime = 60f;
#elif UNITY_PS4 || UNITY_PS5 || UNITY_XBOXONE || UNITY_GAMECORE
        // 主机平台配置
        config.MemoryWarningThresholdMB = 512f;
        config.MemoryCriticalThresholdMB = 768f;
        config.MemoryEmergencyThresholdMB = 1024f;
        config.ReleaseDelayTime = 30f;
#endif

        // 根据设备内存动态调整
        AdjustForDeviceMemory(config);

        return config;
    }

    /// <summary>
    /// 根据设备内存调整配置
    /// </summary>
    private static void AdjustForDeviceMemory(ResourceMemoryConfig memoryConfig)
    {
        int systemMemoryMB = SystemInfo.systemMemorySize;

        if (systemMemoryMB <= 2048) // 2GB或更少
        {
            memoryConfig.MemoryWarningThresholdMB = Mathf.Min(memoryConfig.MemoryWarningThresholdMB, 200f);
            memoryConfig.MemoryCriticalThresholdMB = Mathf.Min(memoryConfig.MemoryCriticalThresholdMB, 300f);
            memoryConfig.ReleaseDelayTime = Mathf.Min(memoryConfig.ReleaseDelayTime, 15f);
        }
        else if (systemMemoryMB >= 8192) // 8GB或更多
        {
            memoryConfig.ReleaseDelayTime = Mathf.Max(memoryConfig.ReleaseDelayTime, 60f);
        }
    }

    #endregion
}

/// <summary>
/// 内存压力级别
/// </summary>
public enum MemoryPressureLevel
{
    /// <summary>
    /// 正常
    /// </summary>
    Normal,

    /// <summary>
    /// 警告 - 开始清理非必要资源
    /// </summary>
    Warning,

    /// <summary>
    /// 严重 - 积极清理资源
    /// </summary>
    Critical,

    /// <summary>
    /// 紧急 - 强制清理所有非常驻资源
    /// </summary>
    Emergency
}

/// <summary>
/// 内存监控信息
/// </summary>
public class MemoryInfo
{
    /// <summary>
    /// 已使用内存（MB）
    /// </summary>
    public float UsedMemoryMB { get; set; }

    /// <summary>
    /// 系统总内存（MB）
    /// </summary>
    public float TotalMemoryMB { get; set; }

    /// <summary>
    /// 内存使用率（0-1）
    /// </summary>
    public float UsageRatio => TotalMemoryMB > 0 ? UsedMemoryMB / TotalMemoryMB : 0;

    /// <summary>
    /// 当前压力级别
    /// </summary>
    public MemoryPressureLevel PressureLevel { get; set; }

    /// <summary>
    /// Mono堆内存（MB）
    /// </summary>
    public float MonoHeapMB { get; set; }

    /// <summary>
    /// Mono已使用内存（MB）
    /// </summary>
    public float MonoUsedMB { get; set; }

    /// <summary>
    /// 图形内存（MB）
    /// </summary>
    public float GraphicsMemoryMB { get; set; }

    public override string ToString()
    {
        return $"Memory: {UsedMemoryMB:F1}MB / {TotalMemoryMB:F1}MB ({UsageRatio:P0}), Level: {PressureLevel}";
    }
}

}

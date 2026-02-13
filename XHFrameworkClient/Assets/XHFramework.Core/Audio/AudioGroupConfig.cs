namespace XHFramework.Core {

/// <summary>
/// 音频组配置（AOT 层定义，热更层填充数据）
/// </summary>
public class AudioGroupConfig
{
    /// <summary>
    /// 音频组名称
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// 是否避免被同优先级声音替换
    /// </summary>
    public readonly bool AvoidBeingReplacedBySamePriority;

    /// <summary>
    /// 是否静音
    /// </summary>
    public readonly bool Mute;

    /// <summary>
    /// 音量
    /// </summary>
    public readonly float Volume;

    /// <summary>
    /// 音频代理数量
    /// </summary>
    public readonly int AgentCount;

    public AudioGroupConfig(string name, bool avoidBeingReplacedBySamePriority, bool mute, float volume, int agentCount)
    {
        Name = name;
        AvoidBeingReplacedBySamePriority = avoidBeingReplacedBySamePriority;
        Mute = mute;
        Volume = volume;
        AgentCount = agentCount;
    }
}

/// <summary>
/// 声音相关常量
/// </summary>
public static class AudioConstant
{
    public const float DefaultTime = 0f;
    public const bool DefaultMute = false;
    public const bool DefaultLoop = false;
    public const int DefaultPriority = 0;
    public const float DefaultVolume = 1f;
    public const float DefaultPitch = 1f;
    public const float DefaultPanStereo = 0f;
    public const float DefaultSpatialBlend = 0f;
    public const float DefaultMaxDistance = 100f;
}

}

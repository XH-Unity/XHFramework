using UnityEngine;

namespace XHFramework.Core {

/// <summary>
/// 播放声音参数
/// </summary>
public class PlayAudioParams:IReference
{
    /// <summary>
    /// 播放位置
    /// </summary>
    public float Time { get; set; } =  AudioConstant.DefaultTime;

    /// <summary>
    /// 在声音组内是否静音
    /// </summary>
    public bool MuteInAudioGroup { get; set; } =  AudioConstant.DefaultMute;

    /// <summary>
    /// 是否循环播放
    /// </summary>
    public bool Loop { get; set; } =  AudioConstant.DefaultLoop;

    /// <summary>
    /// 声音优先级
    /// </summary>
    public int Priority { get; set; } =  AudioConstant.DefaultPriority;

    /// <summary>
    /// 在声音组内音量大小
    /// </summary>
    public float VolumeInAudioGroup { get; set; } =  AudioConstant.DefaultVolume;

    /// <summary>
    /// 声音音调
    /// </summary>
    public float Pitch { get; set; } =  AudioConstant.DefaultPitch;

    /// <summary>
    /// 声音立体声声相
    /// </summary>
    public float PanStereo { get; set; } =  AudioConstant.DefaultPanStereo;

    /// <summary>
    /// 声音空间混合量
    /// </summary>
    public float SpatialBlend { get; set; } =  AudioConstant.DefaultSpatialBlend;

    /// <summary>
    /// 声音最大距离
    /// </summary>
    public float MaxDistance { get; set; } =  AudioConstant.DefaultMaxDistance;

    /// <summary>
    /// 声音位置
    /// </summary>
    public Vector3 WorldPosition { get; set; } = default(Vector3);

    /// <summary>
    /// 声音位置
    /// </summary>
    public Transform BindingEntity { get; set; } = null;

    public static PlayAudioParams Create()
    {
        PlayAudioParams playAudioParams = ReferencePool.Acquire<PlayAudioParams>();
        return playAudioParams;
    }
    
    /// <summary>
    /// 清理播放声音参数。
    /// </summary>
    public void Clear()
    {
        Time =  AudioConstant.DefaultTime;
        MuteInAudioGroup =  AudioConstant.DefaultMute;
        Loop =  AudioConstant.DefaultLoop;
        Priority =  AudioConstant.DefaultPriority;
        VolumeInAudioGroup =  AudioConstant.DefaultVolume;
        Pitch =  AudioConstant.DefaultPitch;
        PanStereo =  AudioConstant.DefaultPanStereo;
        SpatialBlend =  AudioConstant.DefaultSpatialBlend;
        MaxDistance =  AudioConstant.DefaultMaxDistance;
    }
}

}

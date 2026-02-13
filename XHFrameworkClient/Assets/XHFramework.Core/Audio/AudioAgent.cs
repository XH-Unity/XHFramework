using System;
using UnityEngine;

namespace XHFramework.Core {

/// <summary>
/// 声音代理
/// </summary>
public class AudioAgent
{
    #region 字段与属性
    // 序列编号
    public int SerialId { get; set; }
    // 声音代理所在的声音组
    private AudioGroup _audioGroup;
    // 声音代理GameObject
    private GameObject _gameObject;
    // 音频源
    private AudioSource _audioSource;
    // 绑定的实体
    private Transform _bindingEntity;
    // 声音资源
    private AudioClip _audioClip;
    // 声音创建时间
    public DateTime SetAudioAssetTime { get; private set; }
    // 当前是否正在播放
    public bool IsPlaying => _audioSource != null && _audioSource.isPlaying;
    // 是否静音
    public bool Mute => _audioSource != null && _audioSource.mute;
    // 音量大小
    public float Volume => _audioSource != null ? _audioSource.volume : 0f;
    // 播放位置
    public float Time
    {
        get => _audioSource != null ? _audioSource.time : 0f;
        set
        {
            if (_audioSource != null)
            {
                _audioSource.time = value;
            }
        }
    }
    // 在声音组内是否静音
    private bool _muteInAudioGroup;
    public bool MuteInAudioGroup
    {
        get => _muteInAudioGroup;
        set
        {
            _muteInAudioGroup = value;
            RefreshMute();
        }
    }
    // 是否循环播放
    public bool Loop
    {
        get => _audioSource != null && _audioSource.loop;
        set
        {
            if (_audioSource != null)
            {
                _audioSource.loop = value;
            }
        }
    }
    // 声音优先级
    public int Priority
    {
        get => _audioSource != null ? 128 - _audioSource.priority : 0;
        set
        {
            if (_audioSource != null)
            {
                _audioSource.priority = 128 - value;
            }
        }
    }
    // 在声音组内音量大小
    private float _volumeInAudioGroup;
    public float VolumeInAudioGroup
    {
        get => _volumeInAudioGroup;
        set
        {
            _volumeInAudioGroup = value;
            RefreshVolume();
        }
    }
    // 声音音调
    public float Pitch
    {
        get => _audioSource != null ? _audioSource.pitch : 1f;
        set
        {
            if (_audioSource != null)
            {
                _audioSource.pitch = value;
            }
        }
    }
    // 声音立体声声相
    public float PanStereo
    {
        get => _audioSource != null ? _audioSource.panStereo : 0f;
        set
        {
            if (_audioSource != null)
            {
                _audioSource.panStereo = value;
            }
        }
    }
    // 声音空间混合量
    public float SpatialBlend
    {
        get => _audioSource != null ? _audioSource.spatialBlend : 0f;
        set
        {
            if (_audioSource != null)
            {
                _audioSource.spatialBlend = value;
            }
        }
    }
    // 声音最大距离
    public float MaxDistance
    {
        get => _audioSource != null ? _audioSource.maxDistance : 100f;
        set
        {
            if (_audioSource != null)
            {
                _audioSource.maxDistance = value;
            }
        }
    }

    #endregion

    #region 初始化

    /// <summary>
    /// 初始化声音代理
    /// </summary>
    public void Initialize(AudioGroup audioGroup, string agentName, Transform parent)
    {
        _audioGroup = audioGroup;
        SerialId = 0;

        // 创建声音代理GameObject
        _gameObject = new GameObject(agentName);
        _gameObject.transform.SetParent(parent);

        // 创建并配置AudioSource
        _audioSource = _gameObject.GetOrAddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.rolloffMode = AudioRolloffMode.Custom;
        Reset();
    }

    #endregion

    #region Update方法

    /// <summary>
    /// 更新声音代理
    /// </summary>
    public void Update()
    {
        if (_audioSource == null)
        {
            return;
        }

        // 播放完成后重置
        if (!_audioSource.isPlaying && _audioSource.clip != null)
        {
            Reset();
            return;
        }

        // 更新绑定实体位置
        if (_bindingEntity != null)
        {
            if (_bindingEntity.gameObject.activeSelf)
            {
                _gameObject.transform.position = _bindingEntity.position;
            }
            else
            {
                Reset();
            }
        }
    }

    #endregion

    #region 播放声音的相关操作

    /// <summary>
    /// 播放声音
    /// </summary>
    public void Play()
    {
        if (_audioSource != null)
        {
            _audioSource.Play();
        }
    }

    /// <summary>
    /// 停止播放声音
    /// </summary>
    public void Stop()
    {
        if (_audioSource != null)
        {
            _audioSource.Stop();
        }
    }

    /// <summary>
    /// 暂停播放声音
    /// </summary>
    public void Pause()
    {
        if (_audioSource != null)
        {
            _audioSource.Pause();
        }
    }

    /// <summary>
    /// 恢复播放声音
    /// </summary>
    public void Resume()
    {
        if (_audioSource != null)
        {
            _audioSource.UnPause();
        }
    }

    #endregion

    /// <summary>
    /// 设置声音资源
    /// </summary>
    public bool SetAudioAsset(AudioClip audioClip)
    {
        Reset();
        _audioClip = audioClip;
        SetAudioAssetTime = DateTime.Now;
        
        if (audioClip == null)
        {
            return false;
        }

        _audioSource.clip = audioClip;
        return true;
    }

    /// <summary>
    /// 设置声音绑定的实体
    /// </summary>
    public void SetBindingEntity(Transform entity)
    {
        _bindingEntity = entity;
        if (_bindingEntity != null && _bindingEntity.gameObject.activeSelf && _gameObject != null)
        {
            _gameObject.transform.position = _bindingEntity.position;
        }
    }

    /// <summary>
    /// 设置声音所在的世界坐标
    /// </summary>
    public void SetWorldPosition(Vector3 position)
    {
        if (_gameObject != null)
        {
            _gameObject.transform.position = position;
        }
    }

    /// <summary>
    /// 刷新是否静音
    /// </summary>
    public void RefreshMute()
    {
        if (_audioSource != null)
        {
            _audioSource.mute = _audioGroup.Mute || _muteInAudioGroup;
        }
    }

    /// <summary>
    /// 刷新音量大小
    /// </summary>
    public void RefreshVolume()
    {
        if (_audioSource != null)
        {
            _audioSource.volume = _audioGroup.Volume * _volumeInAudioGroup;
        }
    }

    /// <summary>
    /// 重置声音代理
    /// </summary>
    public void Reset()
    {
        if (_audioClip != null)
        {
            FW.ResourceManager.Release(_audioClip);
            _audioClip = null;
        }

        if (_gameObject != null)
        {
            _gameObject.transform.localPosition = Vector3.zero;
        }

        if (_audioSource != null)
        {
            _audioSource.clip = null;
        }

        _bindingEntity = null;
        SetAudioAssetTime = DateTime.MinValue;
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

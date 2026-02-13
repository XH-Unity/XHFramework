using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using YooAsset;

namespace XHFramework.Core {

/// <summary>
/// 声音组
/// </summary>
public class AudioGroup
{
    #region 字段与属性
    /// <summary>
    /// 该组的所有声音代理
    /// </summary>
    private readonly List<AudioAgent> _audioAgents;

    /// <summary>
    /// 是否静音
    /// </summary>
    private bool _mute;

    /// <summary>
    /// 音量大小
    /// </summary>
    private float _volume;

    /// <summary>
    /// 声音组GameObject
    /// </summary>
    private readonly GameObject _gameObject;

    /// <summary>
    /// 声音组中的声音是否避免被同优先级声音替换
    /// </summary>
    public bool AvoidBeingReplacedBySamePriority { get; set; }

    /// <summary>
    /// 声音组静音
    /// </summary>
    public bool Mute
    {
        get => _mute;
        set
        {
            _mute = value;
            foreach (AudioAgent audioAgent in _audioAgents)
            {
                audioAgent.RefreshMute();
            }
        }
    }

    /// <summary>
    /// 声音组音量
    /// </summary>
    public float Volume
    {
        get => _volume;
        set
        {
            _volume = value;
            foreach (AudioAgent audioAgent in _audioAgents)
            {
                audioAgent.RefreshVolume();
            }
        }
    }
    #endregion

    #region 构造方法
    public AudioGroup(string name, Transform parent)
    {
        _audioAgents = new List<AudioAgent>();

        // 创建声音组GameObject
        _gameObject = new GameObject(string.Format("{0}Group - ", name));
        _gameObject.transform.SetParent(parent);
    }
    #endregion

    /// <summary>
    /// 增加声音代理
    /// </summary>
    public void AddAudioAgent(int index)
    {
        // 创建声音agent
        string agentName = string.Format("Agent -  - {0}", index);
        AudioAgent agent = new AudioAgent();
        agent.Initialize(this, agentName, _gameObject.transform);
        _audioAgents.Add(agent);
    }

    /// <summary>
    /// 更新声音组
    /// </summary>
    public void Update()
    {
        foreach (AudioAgent audioAgent in _audioAgents)
        {
            audioAgent.Update();
        }
    }

    #region 播放声音相关的操作
    /// <summary>
    /// 播放声音
    /// </summary>
    /// <param name="serialId">声音的序列编号</param>
    /// <param name="assetHandle">声音资源</param>
    /// <param name="playAudioParams">播放声音参数</param>
    /// <returns>用于播放的声音代理</returns>
    public AudioAgent PlayAudio(int serialId, AudioClip audioClip, PlayAudioParams playAudioParams)
    {
        AudioAgent agent = null;
        foreach (AudioAgent audioAgent in _audioAgents)
        {
            if (!audioAgent.IsPlaying)
            {
                agent = audioAgent;
                break;
            }

            //如果当前声音代理正在播放
            if (audioAgent.Priority < playAudioParams.Priority)
            {
                if (agent == null || audioAgent.Priority < agent.Priority)
                {
                    agent = audioAgent;
                }
            }
            else if (!AvoidBeingReplacedBySamePriority && audioAgent.Priority == playAudioParams.Priority)
            {
                if (agent == null || audioAgent.SetAudioAssetTime < agent.SetAudioAssetTime)
                {
                    agent = audioAgent;
                }
            }
        }

        if (agent == null)
        {
            Log.Error("繁忙，没有可用的agent");
            return null;
        }

        if (!agent.SetAudioAsset(audioClip))
        {
            return null;
        }

        agent.SerialId = serialId;
        agent.Time = playAudioParams.Time;
        agent.MuteInAudioGroup = playAudioParams.MuteInAudioGroup;
        agent.Loop = playAudioParams.Loop;
        agent.Priority = playAudioParams.Priority;
        agent.VolumeInAudioGroup = playAudioParams.VolumeInAudioGroup;
        agent.Pitch = playAudioParams.Pitch;
        agent.PanStereo = playAudioParams.PanStereo;
        agent.SpatialBlend = playAudioParams.SpatialBlend;
        agent.MaxDistance = playAudioParams.MaxDistance;
        
        //绑定的实体或位置
        if (playAudioParams.BindingEntity != null)
        {
            agent.SetBindingEntity(playAudioParams.BindingEntity);
        }
        else
        {
            agent.SetWorldPosition(playAudioParams.WorldPosition);
        }

        //播放声音
        agent.Play();

        return agent;
    }

    /// <summary>
    /// 停止播放声音
    /// </summary>
    /// <param name="serialId">要停止播放声音的序列编号</param>
    /// <returns>是否停止播放声音成功</returns>
    public bool StopAudio(int serialId)
    {
        foreach (AudioAgent audioAgent in _audioAgents)
        {
            if (audioAgent.SerialId != serialId)
            {
                continue;
            }

            audioAgent.Stop();
            return true;
        }

        return false;
    }

    /// <summary>
    /// 暂停播放声音
    /// </summary>
    /// <param name="serialId">要暂停播放声音的序列编号</param>
    /// <returns>是否暂停播放声音成功</returns>
    public bool PauseAudio(int serialId)
    {
        foreach (AudioAgent audioAgent in _audioAgents)
        {
            if (audioAgent.SerialId != serialId)
            {
                continue;
            }

            audioAgent.Pause();
            return true;
        }

        return false;
    }

    /// <summary>
    /// 恢复播放声音
    /// </summary>
    /// <param name="serialId">要恢复播放声音的序列编号</param>
    /// <returns>是否恢复播放声音成功</returns>
    public bool ResumeAudio(int serialId)
    {
        foreach (AudioAgent audioAgent in _audioAgents)
        {
            if (audioAgent.SerialId != serialId)
            {
                continue;
            }

            audioAgent.Resume();
            return true;
        }

        return false;
    }

    /// <summary>
    /// 停止所有已加载的声音
    /// </summary>
    public void StopAllLoadedAudios()
    {
        foreach (AudioAgent audioAgent in _audioAgents)
        {
            if (audioAgent.IsPlaying)
            {
                audioAgent.Stop();
            }
        }
    }
    #endregion
}

}

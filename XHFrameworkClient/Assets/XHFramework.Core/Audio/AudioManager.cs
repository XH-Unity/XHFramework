using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using YooAsset;

namespace XHFramework.Core {

/// <summary>
/// 声音管理器
/// </summary>
public class AudioManager : ManagerBase
{
    public override int Priority => 50;
    private GameObject _gameObject;

    #region 字段与属性

    /// <summary>
    /// 所有声音组的字典
    /// </summary>
    private readonly Dictionary<string, AudioGroup> _audioGroups = new();

    /// <summary>
    /// 正在加载的声音
    /// </summary>
    private readonly List<int> _audiosBeingLoaded = new();

    /// <summary>
    /// 需要在加载后释放的声音
    /// </summary>
    private readonly HashSet<int> _audiosToReleaseOnLoad = new();

    /// <summary>
    /// 序列编号
    /// </summary>
    private int _serial=0;

    #endregion

    #region 生命周期

    public override void Init()
    {
        _gameObject = new GameObject("AudioManager");
        _gameObject.transform.SetParent(FW.Root.transform);
        _gameObject.GetOrAddComponent<AudioListener>();

        Log.Info("AudioManager初始化");
        // AudioGroup 的初始化移到 InitAudioGroups()，由热更层调用
    }

    /// <summary>
    /// 初始化音频组（由热更层调用）
    /// </summary>
    /// <param name="groupConfigs">音频组配置列表</param>
    public void InitAudioGroups(List<AudioGroupConfig> groupConfigs)
    {
        if (groupConfigs == null || groupConfigs.Count == 0)
        {
            Log.Warn("AudioGroupConfigs is null or empty");
            return;
        }

        for (int i = 0; i < groupConfigs.Count; i++)
        {
            AudioGroupConfig config = groupConfigs[i];
            if (!AddAudioGroup(config.Name, config.AgentCount, config.AvoidBeingReplacedBySamePriority, config.Mute, config.Volume))
            {
                Log.Warn("Add audio group '{0}' failure.", config.Name);
            }
        }

        Log.Info("AudioManager 音频组初始化完成，共 {0} 个组", groupConfigs.Count);
    }

    /// <summary>
    /// 关闭并清理声音管理器
    /// </summary>
    public override void Shutdown()
    {
        StopAllLoadedAudios();
        _audioGroups.Clear();
        _audiosBeingLoaded.Clear();
        _audiosToReleaseOnLoad.Clear();
    }

    public override void Update(float elapseSeconds, float realElapseSeconds)
    {
        foreach (AudioGroup audioGroup in _audioGroups.Values)
        {
            audioGroup.Update();
        }
    }

    #endregion

    #region 声音组相关方法

    /// <summary>
    /// 是否存在指定声音组
    /// </summary>
    /// <param name="audioGroupName">声音组名称</param>
    /// <returns>指定声音组是否存在</returns>
    private bool HasAudioGroup(string audioGroupName)
    {
        if (string.IsNullOrEmpty(audioGroupName))
        {
            Log.Error("要检查是否存在的声音组名称为空");
            return false;
        }

        return _audioGroups.ContainsKey(audioGroupName);
    }

    /// <summary>
    /// 获取指定声音组
    /// </summary>
    /// <param name="audioGroupName">声音组名称</param>
    /// <returns>要获取的声音组</returns>
    public AudioGroup GetAudioGroup(string audioGroupName)
    {
        if (string.IsNullOrEmpty(audioGroupName))
        {
            Log.Error("要获取的声音组名称为空");
            return null;
        }

        return _audioGroups.GetValueOrDefault(audioGroupName);
    }
    

    /// <summary>
    /// 增加声音组
    /// </summary>
    /// <param name="audioGroupName">声音组名称</param>
    /// <param name="audioAgentCount">声音代理数量</param>
    /// <param name="audioGroupAvoidBeingReplacedBySamePriority">声音组中的声音是否避免被同优先级声音替换</param>
    /// <param name="audioGroupMute">声音组是否静音</param>
    /// <param name="audioGroupVolume">声音组音量</param>
    /// <returns>是否增加声音组成功</returns>
    private bool AddAudioGroup(string audioGroupName, int audioAgentCount,
        bool audioGroupAvoidBeingReplacedBySamePriority = false, bool audioGroupMute = false,
        float audioGroupVolume = 1f)
    {
        if (HasAudioGroup(audioGroupName))
        {
            Log.Error("要增加的声音组已存在：" + audioGroupName);
            return false;
        }

        if (string.IsNullOrEmpty(audioGroupName))
        {
            Log.Error("要增加的声音组名称为空");
            return false;
        }

        //创建声音组
        AudioGroup audioGroup = new AudioGroup(audioGroupName, _gameObject.transform)
        {
            AvoidBeingReplacedBySamePriority = audioGroupAvoidBeingReplacedBySamePriority,
            Mute = audioGroupMute,
            Volume = audioGroupVolume
        };
        _audioGroups.Add(audioGroupName, audioGroup);

        //添加声音代理
        for (int i = 0; i < audioAgentCount; i++)
        {
            audioGroup.AddAudioAgent(i);
        }

        return true;
    }

    #endregion

    #region 声音相关方法

    /// <summary>
    /// 是否正在加载声音
    /// </summary>
    /// <param name="serialId">声音序列编号</param>
    /// <returns>是否正在加载声音</returns>
    private bool IsLoadingAudio(int serialId)
    {
        return _audiosBeingLoaded.Contains(serialId);
    }

    /// <summary>
    /// 播放声音
    /// </summary>
    /// <param name="audioAssetName">声音资源名称</param>
    /// <param name="audioGroupName">声音组名称</param>
    /// <param name="priority">资源优先级</param>
    /// <param name="playAudioParams">播放声音参数</param>
    /// <returns>声音的序列编号</returns>
    public async UniTask<int?> PlayAudio(string audioAssetName, string audioGroupName,uint priority ,PlayAudioParams playAudioParams = null)
    {
        if (playAudioParams == null)
        {
            playAudioParams = PlayAudioParams.Create();
        }

        int serialId = _serial++;
        //获取声音组
        AudioGroup audioGroup = GetAudioGroup(audioGroupName);
        //加载声音
        _audiosBeingLoaded.Add(serialId);
        AudioClip audioClip=await  FW.ResourceManager.LoadAssetAsync<AudioClip>(audioAssetName,priority);
        //加载结果
        if (audioClip)
        {
            _audiosBeingLoaded.Remove(serialId);
            if (_audiosToReleaseOnLoad.Contains(serialId))
            {
                Log.Error(string.Format("需要释放的声音：{0} 加载成功", serialId));
                _audiosToReleaseOnLoad.Remove(serialId);
                FW.ResourceManager.Release(audioClip);
                ReferencePool.Release(playAudioParams);
                return null;
            }

            //播放声音
            AudioAgent audioAgent = audioGroup.PlayAudio(serialId, audioClip, playAudioParams);

            if (audioAgent == null)
            {
                //绑定的实体或位置
                _audiosToReleaseOnLoad.Remove(serialId);
                FW.ResourceManager.Release(audioClip);
            }
        }
        else
        {
            _audiosToReleaseOnLoad.Remove(serialId);
            Log.Error("播放声音：{0} 失败，错误信息：{1}", audioAssetName);
            FW.ResourceManager.Release(audioClip);
            ReferencePool.Release(playAudioParams);
            return null;
        }
        ReferencePool.Release(playAudioParams);
        return serialId;
    }

    /// <summary>
    /// 暂停播放声音
    /// </summary>
    /// <param name="serialId">要暂停播放声音的序列编号</param>
    public void PauseAudio(int serialId)
    {
        foreach (AudioGroup audioGroup in _audioGroups.Values)
        {
            if (audioGroup.PauseAudio(serialId))
            {
                return;
            }
        }
        Log.Error("没找到要暂停的声音：" + serialId);
    }

    /// <summary>
    /// 恢复播放声音
    /// </summary>
    /// <param name="serialId">要恢复播放声音的序列编号</param>
    public void ResumeAudio(int serialId)
    {
        foreach (AudioGroup audioGroup in _audioGroups.Values)
        {
            if (audioGroup.ResumeAudio(serialId))
            {
                return;
            }
        }

        Log.Error("没找到要恢复的声音：" + serialId);
    }

    #region 停止播放声音

    /// <summary>
    /// 停止播放声音
    /// </summary>
    /// <param name="serialId">要停止播放声音的序列编号</param>
    /// <returns>是否停止播放声音成功</returns>
    public bool StopAudio(int serialId)
    {
        if (IsLoadingAudio(serialId))
        {
            _audiosToReleaseOnLoad.Add(serialId);
            return true;
        }

        foreach (AudioGroup audioGroup in _audioGroups.Values)
        {
            if (audioGroup.StopAudio(serialId))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 停止所有已加载的声音
    /// </summary>
    public void StopAllLoadedAudios()
    {
        foreach (AudioGroup audioGroup in _audioGroups.Values)
        {
            audioGroup.StopAllLoadedAudios();
        }
    }

    /// <summary>
    /// 停止所有正在加载的声音
    /// </summary>
    public void StopAllLoadingAudios()
    {
        foreach (int serialId in _audiosBeingLoaded)
        {
            _audiosToReleaseOnLoad.Add(serialId);
        }
    }

    #endregion

    #endregion



}

}
